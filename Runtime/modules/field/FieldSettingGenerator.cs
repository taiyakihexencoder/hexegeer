using Unity.Entities;

namespace hexegeer {
	public partial class FieldSettingGenerator {
		public static void Generate(EntityManager entityManager, Entity parent) {
			FieldSettingGenerator generator = new FieldSettingGenerator();
			generator.GenerateInternal(entityManager, parent);
		}

		private FieldSettingGenerator() { }

		partial void GenerateInternal(EntityManager entityManager, Entity parent);
	}
}