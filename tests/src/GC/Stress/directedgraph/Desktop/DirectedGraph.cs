// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/************************************************************************************************************

This test does the following:

A directed weighted graph is constructed with 'n' nodes and random edges.
All the nodes are made reachable.
A random node is deleted and the entire graph is restructured by deleting the related edges
and entries in the adjacent-node list.
n is 800 here

*************************************************************************************************************/

namespace DefaultNamespace
{
    using System;

    public class Graph
    {
        private Vertex _vfirst = null;
        private Vertex _vlast = null;
        private Edge _efirst = null;
        private Edge _elast = null;
        private int _weightSum = 0;

        public static int Nodes;


        public Graph(int n) { Nodes = n; }

        public void SetWeightSum()
        {
            Edge temp = _efirst;
            _weightSum = 0;
            while (temp != null)
            {
                _weightSum += temp.Weight;
                temp = temp.Next;
            }
        }

        public int GetWeightSum()
        {
            return _weightSum;
        }

        public void BuildEdge(int v1, int v2)
        {
            Vertex n1 = null, n2 = null;
            Vertex temp = _vfirst;

            while (temp != null)
            {
                if (v1 == temp.Name)
                {
                    //found 1st node..
                    n1 = temp;
                    break;
                }
                else temp = temp.Next;
            }

            //check if edge already exists
            for (int i = 0; i < n1.Num_Edges; i++)
            {
                if (v2 == n1.Adjacent[i].Name)
                    return;
            }

            temp = _vfirst;
            while (temp != null)
            {
                if (v2 == temp.Name)
                {
                    //found 2nd node..
                    n2 = temp;
                    break;
                }
                else temp = temp.Next;
            }

            n1.Adjacent[n1.Num_Edges++] = n2;

            Edge temp2 = new Edge(n1, n2);
            if (_efirst == null)
            {
                _efirst = temp2;
                _elast = temp2;
            }
            else
            {
                temp2.AddEdge(_elast, temp2);
                _elast = temp2;
            }
        }

        public void BuildGraph()
        {
            // Build Nodes	
            Console.WriteLine("Building Vertices...");
            for (int i = 0; i < Nodes; i++)
            {
                Vertex temp = new Vertex(i);
                if (_vfirst == null)
                {
                    _vfirst = temp;
                    _vlast = temp;
                }
                else
                {
                    temp.AddVertex(_vlast, temp);
                    _vlast = temp;
                }
                Console.Write("Vertex ");
                Console.Write(i);
                Console.WriteLine(" built...");
            }

            // Build Edges
            Console.WriteLine("Building Edges...");

            Int32 seed = Environment.TickCount;
            Random rand = new Random(seed);

            for (int i = 0; i < Nodes; i++)
            {
                int j = rand.Next(0, Nodes);
                for (int k = 0; k < j; k++)
                {
                    int v2;
                    while ((v2 = rand.Next(0, Nodes)) == i) ;     //select a random node, also avoid self-loops
                    BuildEdge(i, v2);                //build edge betn node i and v2
                                                     //Console.WriteLine("Edge built between {0} and {1}...",i,v2);


                }
            }
        }


        public void CheckIfReachable()
        {
            int[] temp = new int[Nodes];
            Vertex t1 = _vfirst;

            Console.WriteLine("Making all vertices reachable...");
            while (t1 != null)
            {
                for (int i = 0; i < t1.Num_Edges; i++)
                {
                    if (temp[t1.Adjacent[i].Name] == 0)
                        temp[t1.Adjacent[i].Name] = 1;
                }
                t1 = t1.Next;
            }

            for (int v2 = 0; v2 < Nodes; v2++)
            {
                if (temp[v2] == 0)
                {  //this vertex is not connected
                    Int32 seed = Environment.TickCount;
                    Random rand = new Random(seed);
                    int v1;
                    while ((v1 = rand.Next(0, Nodes)) == v2) ;     //select a random node, also avoid self-loops
                    BuildEdge(v1, v2);
                    temp[v2] = 1;
                }
            }
        }

        /*public void TraverseGraph() {
            Vertex root = Vfirst;
            int i=0,j=0;
            Vertex next = root.Adjacent[i];

            while(j<Nodes) {

                    Console.WriteLine("root: " + root.Name);
                    while(next != null) {
                        Console.WriteLine(next.Name);
                        if(next.Name == j) {break;}
                        next = next.Adjacent[0]; 
                    }
                    i++;
                    if((next = root.Adjacent[i]) == null) {
                        i=0;
                        j++;
                        if(root == Vlast) break;
                        else root = root.Next;
                        next = root.Adjacent[i];

                            }
                }	

        }*/

        public void DeleteVertex()
        {
            Vertex temp1 = null;
            Vertex temp2 = _vfirst;

            Int32 seed = Environment.TickCount;
            Random rand = new Random(seed);

            int j = rand.Next(0, Nodes);
            //Console.WriteLine("Deleting vertex: " + j);

            while (temp2 != null)
            {
                if (j == temp2.Name)
                {
                    if (temp2 == _vfirst)
                    {
                        temp2 = null;
                        _vfirst = _vfirst.Next;
                        break;
                    }
                    temp1.Next = temp2.Next;
                    temp2 = null;
                    break;
                }
                else
                {
                    temp1 = temp2;
                    temp2 = temp2.Next;
                }
            }

            // Restructuring the Graph
            Console.WriteLine("Restructuring the Graph...");
            temp2 = _vfirst;
            while (temp2 != null)
            {
                temp2.DeleteAdjacentEntry(j);
                temp2 = temp2.Next;
            }

            Edge e1 = null;
            Edge e2 = _efirst;
            Edge temp = null;

            while (e2 != null)
            {
                if ((j == e2.v1.Name) || (j == e2.v2.Name))
                {
                    if (e2 == _efirst)
                    {
                        temp = e2;
                        e2 = e2.Next;
                        _efirst = _efirst.Next;
                        temp = null;
                    }
                    else
                    {
                        temp = e1;
                        e1.Next = e2.Next;
                        e2 = e2.Next;
                    }
                }
                else
                {
                    e1 = e2;
                    e2 = e2.Next;
                }
            }
        }

        public void PrintGraph()
        {
            // Print Vertices
            Vertex temp = _vfirst;
            while (temp != null)
            {
                Console.Write("Vertex: ");
                Console.WriteLine(temp.Name);
                Console.WriteLine("Adjacent Vertices:");
                for (int i = 0; i < temp.Num_Edges; i++)
                {
                    Console.WriteLine(temp.Adjacent[i].Name);
                }
                temp = temp.Next;
            }

            //Print Edges
            Edge temp2 = _efirst;
            int edge = 0;
            while (temp2 != null)
            {
                Console.Write("Edge ");
                Console.WriteLine(edge++);
                Console.WriteLine("Weight: {0}, v1: {1}, v2: {2}", temp2.Weight, temp2.v1.Name, temp2.v2.Name);
                temp2 = temp2.Next;
            }
            SetWeightSum();
            Console.WriteLine("Sum of Weights is: {0}", GetWeightSum());
        }
    }

    public class Vertex
    {
        public int Name;
        //public bool Visited = false;

        public Vertex Next;
        public Vertex[] Adjacent;
        public Edge[] Edges;
        public int Num_Edges = 0;

        public Vertex(int val)
        {
            Name = val;
            Next = null;
            Adjacent = new Vertex[Graph.Nodes];
        }

        public void AddVertex(Vertex x, Vertex y)
        {
            x.Next = y;
        }

        public void DeleteAdjacentEntry(int n)
        {
            int temp = Num_Edges;
            for (int i = 0; i < temp; i++)
            {
                if (n == Adjacent[i].Name)
                {
                    for (int j = i; j < Num_Edges; j++)
                        Adjacent[j] = Adjacent[j + 1];
                    Num_Edges--;
                    return;
                }
            }
        }
    }


    public class Edge
    {
        public int Weight;
        public Vertex v1, v2;
        public Edge Next;

        public Edge(Vertex n1, Vertex n2)
        {
            v1 = n1;
            v2 = n2;

            int seed = n1.Name + n2.Name;
            Random rand = new Random(seed);
            Weight = rand.Next(0, 50);
        }

        public void AddEdge(Edge x, Edge y)
        {
            x.Next = y;
        }
    }


    public class Test
    {
        public static int Main()
        {
            Console.WriteLine("Building Graph with 800 vertices...");
            Graph MyGraph = new Graph(800);  // graph with 800 nodes
            MyGraph.BuildGraph();

            Console.WriteLine("Checking if all vertices are reachable...");
            MyGraph.CheckIfReachable();

            //Console.WriteLine("Printing the Graph...");
            //MyGraph.PrintGraph();

            Console.WriteLine("Deleting a random vertex...");
            MyGraph.DeleteVertex();

            //MyGraph.PrintGraph();

            Console.WriteLine("Done");
            Console.WriteLine("Test Passed");
            return 100;
        }
    }
}