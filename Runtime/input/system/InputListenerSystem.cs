using hexegeer.internallib;
using Unity.Entities;
using Unity.Transforms;

namespace hexegeer {
	[UpdateInGroup(typeof(HexegeerInputSystemGroup))]
	public partial class InputListenerSystem : SystemBase {
		private InputControl _input;
		private Entity _inputEntity;

		override protected void OnCreate() {
			_input = new InputControl();
			_input.Enable();

			_inputEntity = EntityManager.CreateEntity(
				EntityManager.CreateArchetype(
					new ComponentType[] {
						ComponentType.ReadWrite<InputMainButton>(),
						ComponentType.ReadWrite<InputSideButton>(),
						ComponentType.ReadWrite<InputMainStick>(),
						ComponentType.ReadWrite<InputSubStick>(),
						ComponentType.ReadWrite<InputPressedEvent>(),
						ComponentType.ReadWrite<InputReleasedEvent>(),
						ComponentType.ReadWrite<Parent>(),
						ComponentType.ReadWrite<LocalTransform>(),
						ComponentType.ReadWrite<LocalToWorld>(),
						ComponentType.ReadWrite<AttachHexegeerTree>(),
					}
				)
			);
			ECS.SetEntityName(EntityManager, _inputEntity, "Input@Hexegeer");
		}

		override protected void OnUpdate() {
			RefRW<InputMainButton> mainButton = SystemAPI.GetComponentRW<InputMainButton>(_inputEntity);
			mainButton.ValueRW.button0 = _input.player.button0.IsPressed();
			mainButton.ValueRW.button1 = _input.player.button1.IsPressed();
			mainButton.ValueRW.button2 = _input.player.button2.IsPressed();
			mainButton.ValueRW.button3 = _input.player.button3.IsPressed();

			RefRW<InputSideButton> sideButton = SystemAPI.GetComponentRW<InputSideButton>(_inputEntity);
			sideButton.ValueRW.bumperL = _input.player.bumperl.IsPressed();
			sideButton.ValueRW.bumperR = _input.player.bumperr.IsPressed();
			sideButton.ValueRW.triggerL = _input.player.triggerl.IsPressed();
			sideButton.ValueRW.triggerR = _input.player.triggerr.IsPressed();
			sideButton.ValueRW.stickL = _input.player.stickl.IsPressed();
			sideButton.ValueRW.stickR = _input.player.stickr.IsPressed();

			RefRW<InputMainStick> mainStick = SystemAPI.GetComponentRW<InputMainStick>(_inputEntity);
			mainStick.ValueRW.value = _input.player.axis0.ReadValue<UnityEngine.Vector2>();
			
			RefRW<InputSubStick> subStick = SystemAPI.GetComponentRW<InputSubStick>(_inputEntity);
			subStick.ValueRW.value = _input.player.axis1.ReadValue<UnityEngine.Vector2>();

			DynamicBuffer<InputPressedEvent> pressed = SystemAPI.GetBuffer<InputPressedEvent>(_inputEntity);
			pressed.Clear();
			if (_input.player.button0.WasPressedThisFrame()) { pressed.Add(Pressed(InputButtonEventKey.Button0)); }
			if (_input.player.button1.WasPressedThisFrame()) { pressed.Add(Pressed(InputButtonEventKey.Button1)); }
			if (_input.player.button2.WasPressedThisFrame()) { pressed.Add(Pressed(InputButtonEventKey.Button2)); }
			if (_input.player.button3.WasPressedThisFrame()) { pressed.Add(Pressed(InputButtonEventKey.Button3)); }
			if (_input.player.bumperl.WasPressedThisFrame()) { pressed.Add(Pressed(InputButtonEventKey.BumperL)); }
			if (_input.player.bumperr.WasPressedThisFrame()) { pressed.Add(Pressed(InputButtonEventKey.BumperR)); }
			if (_input.player.triggerl.WasPressedThisFrame()) { pressed.Add(Pressed(InputButtonEventKey.TriggerL)); }
			if (_input.player.triggerr.WasPressedThisFrame()) { pressed.Add(Pressed(InputButtonEventKey.TriggerR)); }
			if (_input.player.stickl.WasPressedThisFrame()) { pressed.Add(Pressed(InputButtonEventKey.StickL)); }
			if (_input.player.stickr.WasPressedThisFrame()) { pressed.Add(Pressed(InputButtonEventKey.StickR)); }

			DynamicBuffer<InputReleasedEvent> released = SystemAPI.GetBuffer<InputReleasedEvent>(_inputEntity);
			released.Clear();
			if (_input.player.button0.WasReleasedThisFrame()) { released.Add(Released(InputButtonEventKey.Button0)); }
			if (_input.player.button1.WasReleasedThisFrame()) { released.Add(Released(InputButtonEventKey.Button1)); }
			if (_input.player.button2.WasReleasedThisFrame()) { released.Add(Released(InputButtonEventKey.Button2)); }
			if (_input.player.button3.WasReleasedThisFrame()) { released.Add(Released(InputButtonEventKey.Button3)); }
			if (_input.player.bumperl.WasReleasedThisFrame()) { released.Add(Released(InputButtonEventKey.BumperL)); }
			if (_input.player.bumperr.WasReleasedThisFrame()) { released.Add(Released(InputButtonEventKey.BumperR)); }
			if (_input.player.triggerl.WasReleasedThisFrame()) { released.Add(Released(InputButtonEventKey.TriggerL)); }
			if (_input.player.triggerr.WasReleasedThisFrame()) { released.Add(Released(InputButtonEventKey.TriggerR)); }
			if (_input.player.stickl.WasReleasedThisFrame()) { released.Add(Released(InputButtonEventKey.StickL)); }
			if (_input.player.stickr.WasReleasedThisFrame()) { released.Add(Released(InputButtonEventKey.StickR)); }
		}

		private InputPressedEvent Pressed(InputButtonEventKey key) { return new InputPressedEvent { key = key, }; }
		private InputReleasedEvent Released(InputButtonEventKey key) { return new InputReleasedEvent { key = key, }; }
	
		override protected void OnDestroy() {
			if (_inputEntity != Entity.Null && EntityManager.Exists(_inputEntity)) {
				EntityManager.DestroyEntity(_inputEntity);
			}
		}
	}
}
