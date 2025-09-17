using System;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace SharpPDDL
{
    internal class PossibleState
    {
        internal readonly PossibleState PreviousPossibleState;
        internal List<ThumbnailObject> ThumbnailObjects;
        internal List<ThumbnailObject> ChangedThumbnailObjects;
        internal readonly string CheckSum;

        /// <summary>
        /// Root
        /// </summary>
        internal PossibleState(List<ThumbnailObject> NewThumbnailObjects)
        {
            this.PreviousPossibleState = null;
            this.ChangedThumbnailObjects = NewThumbnailObjects;
            this.ThumbnailObjects = new List<ThumbnailObject>(NewThumbnailObjects);
            CheckSum = FigureCheckSum();
        }

        /// <summary>
        /// Next Gen.
        /// </summary>
        internal PossibleState(PossibleState PreviousPossibleState, List<ThumbnailObject> ChangedThumbnailObjects)
        {
            this.PreviousPossibleState = PreviousPossibleState;
            this.ChangedThumbnailObjects = ChangedThumbnailObjects;
            this.ThumbnailObjects = new List<ThumbnailObject>(this.PreviousPossibleState.ThumbnailObjects);

            foreach (ThumbnailObject Change in ChangedThumbnailObjects)
            {
                var index = ThumbnailObjects.FindIndex(TO => TO.OriginalObj == Change.OriginalObj);
                ThumbnailObjects[index] = Change;
            }

            CheckSum = FigureCheckSum();
        }

        private string FigureCheckSum()
        {
            string MD5input = "";

            for (int ThumbnailObjectsCounter = 0; ThumbnailObjectsCounter != ThumbnailObjects.Count; ++ThumbnailObjectsCounter)
            {
                MD5input = MD5input + ThumbnailObjects[ThumbnailObjectsCounter].CheckSum + ";";
            }

            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(MD5input);

            string CheckSum;
            using (MD5 md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                CheckSum = Convert.ToBase64String(hashBytes).Substring(0, 6);
            }

            return CheckSum;
        }

        internal bool Compare(ref PossibleState With)
        {
            if (this.CheckSum != With.CheckSum)
                return false;

            //You cannot merge PossibleState with themself
            if (this.Equals(With))
                return false;

            for (ushort ListCounter = 0; ListCounter != this.ThumbnailObjects.Count; ++ListCounter)
            {
                if (this.ThumbnailObjects[ListCounter].Compare(With.ThumbnailObjects[ListCounter]))
                    continue;
                else
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Merges two <see cref="PossibleState"s/> PreviousPossibleState from function arg becomes forgotten here
        /// </summary>
        /// <param name="Annexed"></param>
        internal void Incorporate(ref PossibleState Annexed)
        {
            foreach (var ChThOh in Annexed.ChangedThumbnailObjects)
            {
                if (this.ChangedThumbnailObjects.Any(ChThOb => ChThOb.OriginalObj.Equals(ChThOh.OriginalObj)))
                    continue;

                this.ChangedThumbnailObjects.Add(ChThOh);
                int rIndex = this.ThumbnailObjects.FindIndex(TO => TO.OriginalObj.Equals(ChThOh.OriginalObj));
                this.ThumbnailObjects[rIndex] = ChThOh;
            }

            Annexed = this;
        }
    }
}
