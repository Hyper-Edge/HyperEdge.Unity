using UnityEngine;


namespace HyperEdge.Sdk.Unity.Flexi.GraphViewEditor
{
    public abstract class VariableObject : ScriptableObject
    {
        public abstract Variable Variable { get; set; }
    }

    /// <summary>
    /// This ScriptableObject is just for using serialization in NodeView
    /// </summary>
    public class VariableObject<T> : VariableObject
    {
        public Variable<T> variable = new Variable<T>();

        public override Variable Variable
        {
            get => variable;
            set
            {
		variable = value as Variable<T>;
                if (variable is null)
                {
                    variable = new Variable<T>();
                }
            }
        }
    }
}
