using System.Collections;

namespace SharpPDDL
{
    sealed class CrisscrossEnum : IEnumerator
    {
        private CrisscrossRefEnum CrisscrossRefEnum;

        public CrisscrossEnum(Crisscross crisscross) => CrisscrossRefEnum = new CrisscrossRefEnum(ref crisscross);

        public object Current => CrisscrossRefEnum.Current;

        public bool MoveNext() => CrisscrossRefEnum.MoveNext();

        public void Reset() => CrisscrossRefEnum.Reset();
    }
}
