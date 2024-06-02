namespace HyperEdge.Sdk.Unity.Flexi
{
    internal class AbilityPool : ObjectPool<Ability>
    {
        internal AbilityPool(AbilityFactory factory, int startSize, PoolExpandMethod expandMethod = PoolExpandMethod.OneAtATime) :
            base(factory, startSize, expandMethod)
        {

        }
    }
}
