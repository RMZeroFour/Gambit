namespace Gambit.Core;

public interface ISerializable
{
	void Serialize (ISaveData saveData);
	bool Deserialize (ILoadData loadData);
}