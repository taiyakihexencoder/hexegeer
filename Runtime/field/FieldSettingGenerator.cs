using Unity.Entities;

namespace hexegeer {
	public partial class FieldSettingGenerator {
		public static void Generate(EntityManager entityManager) {
			FieldSettingGenerator generator = new FieldSettingGenerator();
			generator.GenerateInternal(entityManager);
		}

		private FieldSettingGenerator() { }

		partial void GenerateInternal(EntityManager entityManager);
	}
}