using System.Collections.Generic;
using System.Linq;

namespace Travelling_Salesman_Problem
{
    public static class CitiesAPI
    {
        private static readonly Dictionary<int, string> PostalCodesAPI = new Dictionary<int, string>
                {
                    { 1, "Gniezno" },
                    { 2, "Kraków" },
                    { 3, "Płock" },
                    { 4, "Poznań" },
                    { 5, "Warszawa" },
                    { 6, "Lublin" },
                    { 0, "Koszalin" }
                };

        public static int PostalCodeAPI(string Name)
        {
            return PostalCodesAPI.First(c => c.Value == Name).Key;
        }

        private static readonly int[,] DistanceMatrixAPI =
            {  //Kosz, Gni, Krk, Pło, P-ń,Wawa, Lub
                    {   0, 245, 700, 372, 250, 520, 687}, //Koszalin
                    { 245,   0, 456, 165,  48, 293, 448}, //Gniezno
                    { 700, 456,   0, 364, 458, 290, 304}, //Kraków 
                    { 372, 165, 364,   0, 227, 109, 295}, //Płock
                    { 250,  48, 458, 227,   0, 311, 478}, //Poznań
                    { 520, 293, 290, 109, 311,   0, 173}, //Warszawa
                    { 687, 448, 304, 295, 478, 173,   0}  //Lublin
                };

        public static int DistanceAPI(int S_PostalCode, int D_PostalCode)
        {
            return DistanceMatrixAPI[S_PostalCode, D_PostalCode];
        }
    }
}
