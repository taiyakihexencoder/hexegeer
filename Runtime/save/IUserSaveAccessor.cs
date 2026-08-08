using hexegeer.internallib;

namespace hexegeer {
	public interface IUserSaveAccessor {
		PersistentData.ISerializer<UserSaveParameter> serializer { get; }
		PersistentData.IDeserializer<UserSaveParameter> deserializer { get; }
	}
}