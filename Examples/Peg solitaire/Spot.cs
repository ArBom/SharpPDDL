using System;

namespace Peg_solitaire
{
    class Spot
    {
        const char FullPool = '♦';
        const char EmptyPool = '∙';
        const ConsoleColor Moved = ConsoleColor.DarkMagenta;
        const ConsoleColor Static = ConsoleColor.White;

        public readonly ushort Column;
        public readonly ushort Row;
        private bool _Moved = false;
        private bool _Full;

        public bool Full
        {
            get { return _Full; }
            set
            {
                if (_Full != value)
                    _Moved = true;

                _Full = value;
            }
        }

        public Spot (ushort Column, ushort Row, bool Full = true)
        {
            this.Column = Column;
            this.Row = Row;
            this._Full = Full;
        }

        public void ResetMoved() => _Moved = false;

        public void Draw()
        {
            if (_Moved)
                Console.ForegroundColor = Moved;
            else
                Console.ForegroundColor = Static;

            if (_Full)
                Console.Write(FullPool);
            else
                Console.Write(EmptyPool);
        }
    }
}
