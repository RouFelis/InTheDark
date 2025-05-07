using System.Text;
using UnityEngine;

namespace InTheDark.Prototypes.Editor
{
    public enum ObjectType
    {
        NONE,
        NATIVE_CLASS,
        NATIVE_STRUCT,
        MONO_BEHAVIOUR,
        SCRIPTABLE_OBJECT,
        INTERFACE,
        ENUM
    }

    public class RandomClassNameGenerator : MonoBehaviour
    {
        [SerializeField]
        private ObjectType _type;

        [SerializeField, Range(1, 8)]
        private int _depth;

        [SerializeField]
        private string _generatedName;

        [ContextMenu("Generate")]
        private void GenerateName()
        {
            var builder = new StringBuilder();
            var random = new System.Random();
            var max = GetMaxID();

			var first = _type switch
            {
                ObjectType.NATIVE_CLASS => "NC",
                ObjectType.NATIVE_STRUCT => "NS",
                ObjectType.MONO_BEHAVIOUR => "MB",
				ObjectType.SCRIPTABLE_OBJECT => "SO",
				ObjectType.INTERFACE => "I",
				ObjectType.ENUM => "E",
                _ => string.Empty
			};
            var second = random
                .Next(0, max)
                .ToString($"D{_depth}");

            builder.Append(first);
            builder.Append(second);

			_generatedName = builder.ToString();
		}

        private int GetMaxID()
        {
            var max = 1;

            for (var i = 0; i < _depth; i++)
            {
                max *= 10;
            }

            return max - 1;
        }
    } 
}