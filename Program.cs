/*
 * BinarySimplification
 * Copyright © 2011 Michael Landi
 * http://www.sourcesecure.net
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace Landi
{
    public class BinarySimplification
    {
        #region "Support Classes"

        private class ImplicantRelationship
        {
            public Implicant a { get; set; }
            public Implicant b { get; set; }

            public ImplicantRelationship(Implicant first, Implicant second)
            {
                a = first;
                b = second;
            }
        }

        private class ImplicantCollection : List<Implicant> { }

        private class ImplicantRelationshipCollection : List<ImplicantRelationship> { }

        private class Implicant
        {
            public string Mask { get; set; } //number mask.
            public List<int> Minterms { get; set; }

            public Implicant()
            {
                Minterms = new List<int>(); //original integers in group.
            }

            public string ToChar(int length)
            {
                string strFinal = string.Empty;
                string mask = Mask;

                while (mask.Length != length)
                    mask = "0" + mask;

                for (int i = 0; i < mask.Length; i++)
                {
                    if (mask[i] == '0')
                        strFinal += Convert.ToChar(65 + i) + "'";
                    else if (mask[i] == '1')
                        strFinal += Convert.ToChar(65 + i);
                }

                return strFinal;
            }
        }

        private class ImplicantGroup : Dictionary<int, ImplicantCollection> { }

        #endregion


        #region "Utility Functions"
        
        /*
         * Returns length balanced versions of a string.
         * If given a=010 and b=10101 it will return 00010 and 10101.
         */
        private static void GetBalanced(ref string a, ref string b)
        {
            while (a.Length != b.Length)
            {
                if (a.Length < b.Length)
                    a = "0" + a;
                else
                    b = "0" + b;
            }
        }

        /*
         * Returns the number of binary differences when passed two integers as strings.
         */
        private static int GetDifferences(string a, string b)
        {
            GetBalanced(ref a, ref b);

            int differences = 0;

            for (int i = 0; i < a.Length; i++)
                if (a[i] != b[i])
                    differences++;

            return differences;
        }

        /*
         * Retreives the number of '1' characters in a string.
         */
        private static int GetOneCount(string a)
        {
            int count = 0;

            foreach (char c in a.ToCharArray())
                if (c == '1')
                    count++;

            return count;
        }

        /*
         * Calculates a mask given two input strings.
         * For example when passed a=1001 and b=1101
         * will return 1-01.
         */
        private static string GetMask(string a, string b)
        {
            GetBalanced(ref a, ref b);

            string final = string.Empty;

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                    final += '-';
                else
                    final += a[i];
            }

            return final;
        }

        /*
         * Prints a Matrix to the console for debugging purposes only.
         */
        private static void PrintMatrix(bool[,] matrix, int[] inputs, int count = 0)
        {
            int yLength = matrix.GetLength(0);
            int xLength = matrix.GetLength(1);

            Console.WriteLine();
            Console.WriteLine("PASS #" + count + ":");
            for (int x = 0; x < inputs.Length; x++)
                Console.Write("---");
            Console.WriteLine();
            for (int x = 0; x < inputs.Length; x++)
                Console.Write(" " + (inputs[x] > 9 ? inputs[x].ToString() : inputs[x] + " "));
            Console.WriteLine();
            for (int x = 0; x < inputs.Length; x++)
                Console.Write("---");

            for (int y = 0; y < yLength; y++)
            {
                Console.WriteLine();

                for (int x = 0; x < xLength; x++)
                {
                    if (matrix[y, x])
                        Console.Write(" 1 ");
                    else
                        Console.Write(" 0 ");
                }
            }

            Console.WriteLine();
        }

        #endregion


        #region "Core Functions"

        /*
         * Simplifies a givenset of implicants.
         */
        private static bool Simplify(ref ImplicantCollection implicants)
        {
            /*
             * Group by number of 1's and determine relationships by comparing.
             */
            ImplicantGroup group = Group(implicants);
            ImplicantRelationshipCollection relationships = new ImplicantRelationshipCollection();
            for (int i = 0; i < group.Keys.Count; i++)
            {
                if (i == (group.Keys.Count - 1))
                    break;

                ImplicantCollection thisGroup = group[group.Keys.ElementAt(i)];
                ImplicantCollection nextGroup = group[group.Keys.ElementAt(i + 1)];

                foreach (Implicant a in thisGroup)
                    foreach (Implicant b in nextGroup)
                        if (GetDifferences(a.Mask, b.Mask) == 1)
                            relationships.Add(new ImplicantRelationship(a, b));
            }

            /*
             * For each relationship, find the affected minterms and remove them.
             * Then add a new implicant which simplifies the affected minterms.
             */
            foreach (ImplicantRelationship r in relationships)
            {
                ImplicantCollection rmList = new ImplicantCollection();

                foreach (Implicant m in implicants)
                    if (r.a.Equals(m) || r.b.Equals(m))
                        rmList.Add(m);

                foreach (Implicant m in rmList)
                    implicants.Remove(m);

                Implicant newImplicant = new Implicant();
                newImplicant.Mask = GetMask(r.a.Mask, r.b.Mask);
                newImplicant.Minterms.AddRange(r.a.Minterms);
                newImplicant.Minterms.AddRange(r.b.Minterms);

                bool exist = false;
                foreach (Implicant m in implicants)
                    if (m.Mask == newImplicant.Mask)
                       exist = true;

                if (!exist) //Why am I getting dupes?
                    implicants.Add(newImplicant);
            }

            //Return true if simplification occurred, false otherwise.
            return !(relationships.Count == 0);
        }

        /*
         * Populates a matrix based on a given set of implicants and minterms.
         */
        private static void PopulateMatrix(ref bool[,] matrix, ImplicantCollection implicants, int[] inputs)
        {
            for (int m = 0; m < implicants.Count; m++)
            {
                int y = implicants.IndexOf(implicants[m]);

                foreach (int i in implicants[m].Minterms)
                    for (int index = 0; index < inputs.Length; index++)
                        if (i == inputs[index])
                            matrix[y, index] = true;
            }
        }

        /*
         * Groups binary numbers based on 1's.
         * Stores group in a hashtable associated with a list (bucket) for each group.
         */
        private static ImplicantGroup Group(ImplicantCollection implicants)
        {
            ImplicantGroup group = new ImplicantGroup();
            foreach (Implicant m in implicants)
            {
                int count = GetOneCount(m.Mask);

                if (!group.ContainsKey(count))
                    group.Add(count, new ImplicantCollection());

                group[count].Add(m);
            }

            return group;
        }

        /*
         * Retreives the final simplified expression in readable format.
         */
        private static string GetFinalExpression(ImplicantCollection implicants)
        {
            int longest = 0;
            string final = string.Empty;

            foreach (Implicant m in implicants)
                if (m.Mask.Length > longest)
                    longest = m.Mask.Length;

            for (int i = implicants.Count - 1; i >= 0; i--)
                final += implicants[i].ToChar(longest) + " + ";

            return (final.Length > 3 ? final.Substring(0, final.Length - 3) : final);
        }

        /*
         * Selects the smallest group of implicants which satisfy the equation from the matrix.
         */
        private static ImplicantCollection SelectImplicants(ImplicantCollection implicants, int[] inputs)
        {
            List<int> lstToRemove = new List<int>(inputs);
            ImplicantCollection final = new ImplicantCollection();

            while (lstToRemove.Count != 0)
            {
                //Implicant[] weightedTerms = WeightImplicants(implicants, final, lstToRemove);
                foreach (Implicant m in implicants)
                {
                    bool add = false;
                    foreach (int i in m.Minterms)
                        if (lstToRemove.Contains(i))
                            add = true;

                    if (add)
                    {
                        final.Add(m);
                        foreach (int r in m.Minterms)
                            lstToRemove.Remove(r);
                        break;
                    }
                }
            }

            return final;
        }

        /*
         * Entry point.
         */
        public static void Main(string[] args)
        {
            int[] input = null;

            Console.WriteLine("Michael Landi\t\tComp. Architecture.");
            Console.WriteLine("Boolean Simplification Program");
            Console.WriteLine();
            Console.Write("ENTER MINTERMS: ");
            string[] minterms = Console.ReadLine().Split(' ');
            Console.WriteLine();

            //Parse and validate input.
            try
            {
                input = new int[minterms.Length];
                for (int i = 0; i < input.Length; i++)
                    input[i] = -1; //That way we can check for duplicate zeroes.

                //Empty string passed.
                if (minterms.Length == 0 || minterms[0].Trim() == string.Empty)
                    throw new Exception("No input.");

                for (int i = 0; i < minterms.Length; i++)
                {
                    int check = Int32.Parse(minterms[i]);

                    if (check < 0)
                        throw new Exception("Input cannot be less than zero.");
                    if (input.Contains(check))
                        throw new Exception("Input cannot contain the same minterm twice.");

                    input[i] = check;
                }
                Array.Sort(input);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR: " + ex.Message);
                Environment.Exit(-1);
            }

            //Create initial list of  minterms.
            ImplicantCollection implicants = new ImplicantCollection();
            foreach (int i in input)
            {
                Implicant m = new Implicant();
                m.Mask = Convert.ToString(i, 2);
                m.Minterms.Add(i);
                implicants.Add(m);
            }

            //Simplify expressions.
            int count = 0;
            while (Simplify(ref implicants))
            {
                //Populate a matrix.
                bool[,] matrix = new bool[implicants.Count, input.Length]; //x, y
                PopulateMatrix(ref matrix, implicants, input);
                PrintMatrix(matrix, input, ++count);
            }

            //Select implicants.
            ImplicantCollection selected = SelectImplicants(implicants, input);
            string strFinal = GetFinalExpression(selected);

            Console.WriteLine();
            Console.WriteLine("SIMPLIFIED EXPRESSION: ");
            Console.WriteLine(strFinal);
            Console.ReadKey();
        }

        #endregion
    }
} //EOF
