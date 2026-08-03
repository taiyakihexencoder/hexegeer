using hexegeer.internallib;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace hexegeer {
	[UpdateInGroup(typeof(HexegeerBeforePhysicsSystemGroup))]
	public partial struct CharacterPhysicsSystem : ISystem {
		private const float SNAP_TO_GROUND_CAST_LENGTH = 0.1f;
		private const float MOVE_EPSILON = 0.01f;

		private EntityQuery _overwriteSnapToGroundQuery;
		private EntityQuery _groundedQuery;
		private EntityQuery _snapToGroundQuery;

		private EntityQuery _gravityCorrectionQuery;
		private EntityQuery _moveCalculateQuery;

		private EntityQuery _applyPhysicsQuery;
		private EntityQuery _overwriteActionQuery;


		private EntityQuery _syncQuery;
		private EntityQuery _resetOverwriteQuery;
		private EntityQuery _resetQuery;

		void ISystem.OnCreate(ref SystemState state) {
			_overwriteSnapToGroundQuery = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<CharacterPhysicsOverwrite>()
				.WithAllRW<CharacterGroundedStatus>()
				.Build(ref state);

			_groundedQuery = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<ColliderCollisionStayEvent, PhysicsCollider, LocalToWorld>()
				.WithAllRW<CharacterGroundedStatus>()
				.Build(ref state);

			_snapToGroundQuery = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<CharacterGroundedStatus>()
				.WithAllRW<LocalTransform>()
				.Build(ref state);


			_gravityCorrectionQuery = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<PhysicsMass, PhysicsGravityFactor, CharacterGroundedStatus>()
				.WithAllRW<CharacterMoveStatus>()
				.Build(ref state);

			_moveCalculateQuery = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<PhysicsVelocity, CharacterMove>()
				.WithAllRW<CharacterMoveStatus>()
				.Build(ref state);


			_applyPhysicsQuery = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<CharacterMoveStatus, PhysicsMass>()
				.WithAllRW<PhysicsVelocity>()
				.Build(ref state);

			_overwriteActionQuery = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<CharacterPhysicsOverwrite>()
				.WithAllRW<CharacterMoveStatus>()
				.Build(ref state);


			_syncQuery = new EntityQueryBuilder(Allocator.Temp)
				.WithAll<CharacterGroundedStatus, CharacterMoveStatus>()
				.WithAllRW<CharacterPhysical>()
				.Build(ref state);

			_resetOverwriteQuery = new EntityQueryBuilder(Allocator.Temp)
				.WithAllRW<CharacterPhysicsOverwrite>()
				.Build(ref state);

			_resetQuery = new EntityQueryBuilder(Allocator.Temp)
				.WithAllRW<CharacterMoveStatus>()
				.Build(ref state);

			state.RequireForUpdate(_applyPhysicsQuery);
		}

		void ISystem.OnUpdate(ref SystemState state) {
			if (
				SystemAPI.TryGetSingleton(out PhysicsStep physicsStep) &&
				SystemAPI.TryGetSingleton(out PhysicsWorldSingleton physicsWorld)
			) {
				float3 gravity = physicsStep.Gravity;
				float3 upward = math.normalize(-gravity);
				float dt = SystemAPI.Time.DeltaTime;
				CollisionWorld collisionWorld = physicsWorld.CollisionWorld;

				// -- 接地情報の更新 -- //
				if (!_groundedQuery.IsEmpty && !_snapToGroundQuery.IsEmpty) {
					// 状態を上書き制御する
					if (!_overwriteSnapToGroundQuery.IsEmpty) {
						state.Dependency = new OverwriteSnapToGroundJob {

						}.ScheduleParallel(_overwriteSnapToGroundQuery, state.Dependency);
					}

					// 物理的な衝突チェック
					state.Dependency = new UpdateIsGroundedJob {
						upward = upward,
					}.ScheduleParallel(_groundedQuery, state.Dependency);

					// 地面からある程度までは接地状態とみなす
					state.Dependency = new GroundRaycastJob {
						upward = upward,
						castLength = SNAP_TO_GROUND_CAST_LENGTH,
						collisionWorld = collisionWorld,
					}.ScheduleParallel(_groundedQuery, state.Dependency);

					// 移動
					state.Dependency = new SnapToGroundJob {

					}.ScheduleParallel(_snapToGroundQuery, state.Dependency);
				}

				// -- 外力による操作 -- //

				// 坂道による重力補正
				if (!_gravityCorrectionQuery.IsEmpty) {
					state.Dependency = new SlopeGravityJob {
						gravity = gravity,
						dt = dt,
					}.ScheduleParallel(_gravityCorrectionQuery, state.Dependency);
				}

				// 入力による移動の変更
				if (!_moveCalculateQuery.IsEmpty) {
					state.Dependency = new CalculateDynamicVelocityChangeJob {
						upward = upward,
						dt = dt,
						epsilon = MOVE_EPSILON,
					}.ScheduleParallel(_moveCalculateQuery, state.Dependency);
				}

				// 移動関連の上書き制御
				if (!_overwriteActionQuery.IsEmpty) {
					state.Dependency = new OverwriteActionJob {
						
					}.ScheduleParallel(_overwriteActionQuery, state.Dependency);
				}

				// -- 計算結果の反映 -- //

				// 最終的な結果をPhysicsに反映
				state.Dependency = new ExecForceJob {

				}.ScheduleParallel(_applyPhysicsQuery, state.Dependency);


				// -- 終了処理 -- //

				// Physicalに状態の一部を共有
				if (!_syncQuery.IsEmpty) {
					state.Dependency = new SyncParameterJob {

					}.ScheduleParallel(_syncQuery, state.Dependency);
				}

				if (!_resetOverwriteQuery.IsEmpty) {
					state.Dependency = new ResetOverwriteJob {

					}.ScheduleParallel(_resetOverwriteQuery, state.Dependency);
				}

				// パラメーターのリセット
				state.Dependency = new ResetStatusJob {
					
				}.ScheduleParallel(_resetQuery, state.Dependency);
			}

		}
	
		void ISystem.OnDestroy(ref SystemState state) {
		}

		private readonly EntityCommandBuffer CreateCommandBuffer(ref SystemState state) {
			return SystemAPI
				.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
				.CreateCommandBuffer(state.World.Unmanaged);
		}

		/// <summary>
		/// 接地判定の更新
		/// </summary>
		partial struct UpdateIsGroundedJob : IJobEntity {
			[ReadOnly]
			public float3 upward;

			void Execute(
				ref DynamicBuffer<ColliderCollisionStayEvent> evts,
				RefRW<CharacterGroundedStatus> groundedStatus
			) {
				float current = groundedStatus.ValueRO.groundThreshold;
				float cos;
				bool isGrounded = false;
				float3 normal = groundedStatus.ValueRO.normal;

				// よりupwardに対して垂直な平面を接地床とする
				foreach(ColliderCollisionStayEvent evt in evts) {
					cos = math.dot(evt.Normal, upward);
					if (cos > current) {
						current = cos;
						normal = evt.Normal;
						isGrounded = true;
					}
				}

				groundedStatus.ValueRW.physicallyGrounded = isGrounded;
				if (isGrounded) {
					// 吸着機能を有効化
					groundedStatus.ValueRW.snapToGround = true;

					// 法線を更新
					groundedStatus.ValueRW.normal = normal;
				}
			}
		}

		/// <summary>
		/// 地面への吸着処理の有効判定。
		/// 吸着処理によって斜面の切れ目で一瞬浮いてしまう状態を防ぐ
		/// </summary>
		partial struct GroundRaycastJob : IJobEntity {
			[ReadOnly]
			public float3 upward;

			[ReadOnly]
			public float castLength;
			
			[ReadOnly]
			public CollisionWorld collisionWorld;

			void Execute(
				RefRO<LocalToWorld> localToWorld,
				RefRO<PhysicsCollider> physicsCollider,
				RefRW<CharacterGroundedStatus> groundedStatus
			) {
				// 物理的に接地しているなら判定は不要
				if (groundedStatus.ValueRO.physicallyGrounded) { return; }

				// 非接地状態であれば判定は不要
				if (!groundedStatus.ValueRO.snapToGround || groundedStatus.ValueRO.ignoreSnapToGround) {
					return;
				}

				unsafe {
					if (physicsCollider.ValueRO.ColliderPtr->Type != ColliderType.Capsule) { return; }

					float threshold = groundedStatus.ValueRO.groundThreshold;

					CapsuleCollider* capsule = (CapsuleCollider*) physicsCollider.ValueRO.ColliderPtr;
					float radius = capsule->Geometry.Radius;

					float3 start = localToWorld.ValueRO.Position + upward * radius;
					float3 end = start - upward * castLength;

					ColliderCastInput cast = new ColliderCastInput {
						Collider = physicsCollider.ValueRO.ColliderPtr,
						Orientation = quaternion.identity,
						Start = start,
						End = end,
					};

					if (collisionWorld.CastCollider(cast, out ColliderCastHit hit)) {
						float3 normal = hit.SurfaceNormal;

						// 床判定
						if (math.dot(normal, upward) > threshold) {
							groundedStatus.ValueRW.normal = normal;

							// 吸着処理
							float3 delta = math.lerp(start, end, hit.Fraction) - start;
							groundedStatus.ValueRW.translate = math.mul(localToWorld.ValueRO.Value, new float4(delta, 0.0f)).xyz;
						} else {
							// 床以外のものにぶつかっている: 次の接地まで処理は不要
							groundedStatus.ValueRW.snapToGround = false;
						}
					} else {
						// 接地していない: 次の接地まで処理は不要
						groundedStatus.ValueRW.snapToGround = false;
					}
				}
			}
		}

		/// <summary>
		/// 計算された接地吸着先へ移動
		/// </summary>
		partial struct SnapToGroundJob : IJobEntity {
			void Execute(
				RefRO<CharacterGroundedStatus> groundedStatus,
				RefRW<LocalTransform> localTransform
			) {
				localTransform.ValueRW = localTransform.ValueRO.Translate(groundedStatus.ValueRO.translate);
			}
		}

		/// <summary>
		/// 接地状態をphysicalにコピー
		/// </summary>
		partial struct SyncGroundedJob : IJobEntity {
			void Execute(
				RefRO<CharacterGroundedStatus> groundedStatus,
				RefRW<CharacterPhysical> physical
			) {
				// 物理的に床と衝突しているか、snapToGroundが有効の間は接地とみなす
				physical.ValueRW.isGrounded = !groundedStatus.ValueRO.ignoreSnapToGround &&
					(groundedStatus.ValueRO.physicallyGrounded || groundedStatus.ValueRO.snapToGround);
				physical.ValueRW.normal = groundedStatus.ValueRO.normal;
			}
		}

		/// <summary>
		/// 接地時に重力を坂道方向にかけなおすことで坂道の滑りを防ぐ
		/// </summary>
		partial struct SlopeGravityJob : IJobEntity {
			[ReadOnly]
			public float3 gravity;
			[ReadOnly]
			public float dt;

			void Execute(
				RefRO<PhysicsMass> physicsMass,
				RefRO<PhysicsGravityFactor> physicsGravityFactor,
				RefRO<CharacterGroundedStatus> groundedStatus,
				RefRW<CharacterMoveStatus> moveStatus
			) {
				bool grounded = !groundedStatus.ValueRO.ignoreSnapToGround &&
					(groundedStatus.ValueRO.physicallyGrounded || groundedStatus.ValueRO.snapToGround);
				if (grounded) {
					float3 normal = groundedStatus.ValueRO.normal;
					float3 correction = normal * math.dot(normal, gravity) - gravity;
					float factor = physicsGravityFactor.ValueRO.Value;
					float mass = 1.0f / physicsMass.ValueRO.InverseMass;

					moveStatus.ValueRW.force += correction * factor * mass;
				}
			}
		}

		/// <summary>
		/// 入力された移動方向を反映する
		/// </summary>
		partial struct CalculateDynamicVelocityChangeJob : IJobEntity {
			[ReadOnly]
			public float epsilon;
			
			[ReadOnly]
			public float dt;

			[ReadOnly]
			public float3 upward;

			// フレームレートをfとして、
			// t秒後に1がε以下になる減衰比率pを考える
			// 漸化式はステップをsとすると、sMax = ftかつ、
			// a(s+1) = (1-p)a(s), a(0) = 1であり、つまり公比1-pの等比数列。
			// よって(1-p)^ft < εとなるようにする
			// これを解くと、p > 1-ε^(1/(ft))
			// つまりp = 1 - ε^(1/(ft)) = 1 - ε^(dt/t)となるようにpを決めれば、
			// t程度の時間でεまで減衰することになる

			void Execute(
				RefRO<PhysicsVelocity> velocity, 
				RefRO<CharacterMove> move,
				RefRW<CharacterMoveStatus> moveStatus
			) {
				// 移動量の変更

				// 動かしたい操作と現状との差分
				float3 diff = move.ValueRO.move - velocity.ValueRO.Linear;

				// 指定した座標系に射影
				float3x3 matrix = new float3x3(move.ValueRO.xAxis, move.ValueRO.yAxis, move.ValueRO.zAxis);
				float3 diffProj = math.mul(matrix, diff);

				// 補正係数を計算
				float alpha = 1.0f - math.pow(epsilon, dt / moveStatus.ValueRO.correctionSeconds);

				// 補正ベクトルを元の座標系に
				float3 dv = math.mul(diffProj * alpha, math.transpose(matrix));

				moveStatus.ValueRW.velocityChanges += dv;

				// 方向の変更
				float sqrPreferMove = math.lengthsq(move.ValueRO.move);
				float sqrThreshold = moveStatus.ValueRO.lookDirectionThreshold * moveStatus.ValueRO.lookDirectionThreshold;
				if (sqrPreferMove < sqrThreshold) {
					moveStatus.ValueRW.lookDirection = quaternion.LookRotation(move.ValueRO.move, upward);
				}
			}
		}

		/// <summary>
		/// SnapToGroundを上書き
		/// </summary>
		partial struct OverwriteSnapToGroundJob : IJobEntity {
			void Execute(RefRO<CharacterPhysicsOverwrite> overwrite, RefRW<CharacterGroundedStatus> groundedStatus) {
				if (overwrite.ValueRO.overwriteSnapToGround) {
					groundedStatus.ValueRW.snapToGround = overwrite.ValueRO.snapToGround;
				}

				if (overwrite.ValueRO.overwriteIgnoreSnapToGround) {
					groundedStatus.ValueRW.ignoreSnapToGround = overwrite.ValueRO.ignoreSnapToGround;
				}
			}
		}

		/// <summary>
		/// 上書き系の特殊アクション処理
		/// </summary>
		partial struct OverwriteActionJob : IJobEntity {
			void Execute(
				RefRO<CharacterPhysicsOverwrite> overwrite,
				RefRW<CharacterMoveStatus> moveStatus
			) {
				if (overwrite.ValueRO.overwriteVelocity) {
					moveStatus.ValueRW.velocityChanges = overwrite.ValueRO.velocity;
				}
			}
		}

		/// <summary>
		/// システムから計算した力を物理オブジェクトに適用
		/// </summary>
		partial struct ExecForceJob : IJobEntity {
			[ReadOnly]
			public float dt;

			void Execute(
				RefRO<PhysicsMass> physicsMass,
				RefRO<CharacterMoveStatus> moveStatus,
				RefRW<PhysicsVelocity> velocity
			) {
				float3 delta = moveStatus.ValueRO.force * dt * physicsMass.ValueRO.InverseMass;
				velocity.ValueRW.Linear += delta + moveStatus.ValueRO.velocityChanges;
			}
		}

		/// <summary>
		/// physicalにコピー
		/// </summary>
		partial struct SyncParameterJob : IJobEntity {
			void Execute(
				RefRO<CharacterGroundedStatus> groundedStatus,
				RefRO<CharacterMoveStatus> moveStatus,
				RefRW<CharacterPhysical> physical
			) {
				// 物理的に床と衝突しているか、snapToGroundが有効の間は接地とみなす
				physical.ValueRW.isGrounded = !groundedStatus.ValueRO.ignoreSnapToGround &&
					(groundedStatus.ValueRO.physicallyGrounded || groundedStatus.ValueRO.snapToGround);
				physical.ValueRW.normal = groundedStatus.ValueRO.normal;

				physical.ValueRW.rotation = moveStatus.ValueRO.lookDirection;
			}
		}

		partial struct ResetOverwriteJob : IJobEntity {
			void Execute(
				RefRW<CharacterPhysicsOverwrite> overwrite
			) {
				overwrite.ValueRW.overwriteVelocity = false;
				overwrite.ValueRW.overwriteSnapToGround = false;
				overwrite.ValueRW.overwriteIgnoreSnapToGround = false;
			}
		}

		partial struct ResetStatusJob : IJobEntity {
			void Execute(
				RefRW<CharacterMoveStatus> moveStatus
			) {
				moveStatus.ValueRW.force = float3.zero;
				moveStatus.ValueRW.velocityChanges = float3.zero;
			}
		}
	}
}
