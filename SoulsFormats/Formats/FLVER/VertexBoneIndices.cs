using System;
using System.Collections;
using System.Collections.Generic;

namespace SoulsFormats
{
    public partial class FLVER
    {
        /// <summary>
        /// Four indices of bones to bind a vertex to, accessed like an array. Unused bones should be set to 0.
        /// </summary>
<<<<<<< HEAD
        public struct VertexBoneIndices : IEnumerable
=======
        public struct VertexBoneIndices: IEnumerable<int>
>>>>>>> b2ecbfe895c8f7619366a0ee196e6bfa9b08c39b
        {
            private int A, B, C, D;

            /// <summary>
            /// Length of bone indices is always 4.
            /// </summary>
            public int Length => 4;

            /// <summary>
            /// Accesses bone indices as an int[4].
            /// </summary>
            public int this[int i]
            {
                get
                {
                    switch (i)
                    {
                        case 0: return A;
                        case 1: return B;
                        case 2: return C;
                        case 3: return D;
                        default:
                            throw new IndexOutOfRangeException($"Index ({i}) was out of range. Must be non-negative and less than 4.");
                    }
                }

                set
                {
                    switch (i)
                    {
                        case 0: A = value; break;
                        case 1: B = value; break;
                        case 2: C = value; break;
                        case 3: D = value; break;
                        default:
                            throw new IndexOutOfRangeException($"Index ({i}) was out of range. Must be non-negative and less than 4.");
                    }
                }
            }

<<<<<<< HEAD
            private List<int> VBIs => new List<int> {A, B, C, D};
            public IEnumerator<int> GetEnumerator() => VBIs.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
=======
            /// <summary>
            /// Enumerates through all four bone indicies
            /// </summary>
            /// <returns></returns>
            public IEnumerator<int> GetEnumerator()
            {
                yield return A;
                yield return B;
                yield return C;
                yield return D;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
>>>>>>> b2ecbfe895c8f7619366a0ee196e6bfa9b08c39b
        }
    }
}
