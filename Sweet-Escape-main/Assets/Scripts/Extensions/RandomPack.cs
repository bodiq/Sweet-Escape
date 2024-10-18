using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Extensions
{
    /// <summary>
    /// It should be serialized by [OdinSerialize].
    /// </summary>
    [Serializable] [HideReferenceObjectPicker]
    public class RandomPack<T> : IEnumerable<T>
    {
        [Serializable] [HideReferenceObjectPicker]
        public class Element
        {
            [HorizontalGroup] [HideLabel] [OdinSerialize] [InlineProperty] public T value;
            [HideInInspector] [OdinSerialize] public float chance = 0.5f;
            [HorizontalGroup] [HideLabel] [SuffixLabel("%")] [ShowInInspector] [PropertyRange(0, 100)] private float ChanceX100 { get => chance * 100.0f; set => chance = value / 100.0f; }

            public Element()
            {
            }

            public Element(T value, float chance = 0.5f)
            {
                this.value = value;
                this.chance = chance;
            }

            public static T Value(Element element)
            {
                return element.value;
            }

            public static float Chance(Element element)
            {
                return element.chance;
            }

            public static float RandomizedChance(Element element)
            {
                return element.chance * Random.value;
            }

            public static int Comparison(Element elementX, Element elementY)
            {
                return elementX.chance > elementY.chance ? 1 : -1;
            }

            public static int DescendingComparison(Element elementX, Element elementY)
            {
                return elementX.chance > elementY.chance ? -1 : 1;
            }
        }

        #region Inspector

        [ListDrawerSettings(AlwaysAddDefaultValue = true, CustomAddFunction = nameof(CustomAddFunction))]
        [OdinSerialize] public readonly List<Element> Elements = new();

        [OnInspectorInit]
        private void OnInspectorInit()
        {
            SortByDescending();
        }

        private Element CustomAddFunction()
        {
            return new Element();
        }

        #endregion

        public IEnumerator<T> GetEnumerator()
        {
            return Elements.Select(Element.Value).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Elements should be sorted before using this function.
        /// </summary>
        /// <returns> a random element by its normalized chance. </returns>
        /// <example>
        /// value0 = 100% => 40% <br/>
        /// value1 = 100% => 40% <br/>
        /// value2 = 50% => 20% <br/>
        /// </example>
        private Element GetRandomElement(float random)
        {
            var sum = Elements.Sum(Element.Chance);
            random *= sum;
            var total = 0.0f;
            foreach (var element in Elements)
            {
                total += element.chance;
                if (total >= random)
                {
                    return element;
                }
            }

            return default;
        }

        /// <returns> a random value by its normalized chance. </returns>
        /// <example>
        /// value0 = 100% => 40% <br/>
        /// value1 = 100% => 40% <br/>
        /// value2 = 50% => 20% <br/>
        /// </example>
        public T GetRandomValue()
        {
            return GetRandomValue(Random.value);
        }

        /// <returns> a random value by its normalized chance. </returns>
        /// <example>
        /// value0 = 100% => 40% <br/>
        /// value1 = 100% => 40% <br/>
        /// value2 = 50% => 20% <br/>
        /// </example>
        public T GetRandomValue(float randomValue)
        {
            Sort();

            var element = GetRandomElement(randomValue);
            return element.value;
        }

        /// <returns> random values by their normalized chance. </returns>
        /// <example>
        /// value0 = 100% => 40% <br/>
        /// value1 = 100% => 40% <br/>
        /// value2 = 50% => 20% <br/>
        /// </example>
        public IEnumerable<T> GetRandomValues(int count)
        {
            Sort();

            var values = new List<T>(count);
            for (var i = 0; i < count; i++)
            {
                var element = GetRandomElement(Random.value);
                values.Add(element.value);
            }

            return values;
        }

        /// <returns> random values by their chance. </returns>
        /// <example>
        /// value0 = 100% <br/>
        /// value1 = 100% <br/>
        /// value2 = 50% <br/>
        /// </example>
        public IEnumerable<T> GetRandomValues()
        {
            foreach (var element in Elements)
            {
                if (element.chance >= Random.value)
                {
                    yield return element.value;
                }
            }
        }

        [Button]
        public void Normalize()
        {
            var sum = Elements.Sum(Element.Chance);
            if (sum <= 0.0f)
            {
                return;
            }

            foreach (var element in Elements)
            {
                element.chance /= sum;
            }
        }

        public void Sort()
        {
            Elements.Sort(Element.Comparison);
        }

        public void SortByDescending()
        {
            Elements.Sort(Element.DescendingComparison);
        }
    }
}
