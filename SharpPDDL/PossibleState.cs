﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace SharpPDDL
{
    internal class PossibleState
    {
        internal PossibleState PreviousPossibleState;
        internal List<PossibleStateThumbnailObject> ThumbnailObjects;
        internal List<PossibleStateThumbnailObject> ChangedThumbnailObjects;
        internal string CheckSum;

        /// <summary>
        /// Root
        /// </summary>
        internal PossibleState(List<PossibleStateThumbnailObject> NewThumbnailObjects)
        {
            this.PreviousPossibleState = null;
            this.ChangedThumbnailObjects = NewThumbnailObjects;
            this.ThumbnailObjects = new List<PossibleStateThumbnailObject>(NewThumbnailObjects);
            FigureCheckSum();
        }

        /// <summary>
        /// Next Gen.
        /// </summary>
        internal PossibleState(PossibleState PreviousPossibleState, List<PossibleStateThumbnailObject> ChangedThumbnailObjects)
        {
            this.PreviousPossibleState = PreviousPossibleState;
            this.ChangedThumbnailObjects = ChangedThumbnailObjects;
            this.ThumbnailObjects = new List<PossibleStateThumbnailObject>(this.PreviousPossibleState.ThumbnailObjects);

            foreach (PossibleStateThumbnailObject Change in ChangedThumbnailObjects)
            {
                var index = ThumbnailObjects.FindIndex(TO => TO.OriginalObj == Change.OriginalObj);
                ThumbnailObjects[index] = Change;
            }

            FigureCheckSum();
        }

        internal void FigureCheckSum()
        {
            string MD5input = "";

            for (int ThumbnailObjectsCounter = 0; ThumbnailObjectsCounter != ThumbnailObjects.Count; ++ThumbnailObjectsCounter)
            {
                MD5input = MD5input + ThumbnailObjects[ThumbnailObjectsCounter].CheckSum + ";";
            }

            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(MD5input);

            using (MD5 md5 = MD5.Create())
            {
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                CheckSum = Convert.ToBase64String(hashBytes).Substring(0, 6);
            }
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
