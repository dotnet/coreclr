/******************************
 * CircularRef.cs
 * 
 * Tests the effect of having circular references by using 
 * a circular linked list. Test that adding a node to the list increase the size of the list
 * by the size of a node. Test that removing  a node decreases the list size by the size of a node.
 * 
 * The test uses only one SizedReference for the head of the list.
 * 
 * 
 * *****************************/

using System;

public class Node
{
    public Node(int size)
    {
        data = new byte[size];
        next = null;
        prev = null;
    }

    public Node()
    {
        data = null;
        next = null;
        prev = null;
    }

    //insert a node to the head
    static public void InsertToHead(Node head, Node newNode)
    {
        newNode.next = head.next;
        newNode.prev = head;

        if(head.next != null)
        {
            head.next.prev = newNode;
        }
        head.next = newNode;
    }

    //remove a node from head
    static public void Remove(Node head)
    {
        if (head.next == null)
            return;

        if (head.next.next != null)
        {
            head.next.next.prev = head;
            head.next = head.next.next;
        }
        else
        {
            head.next = null;
        }
       
    }

    public byte[] data;
    public Node next;
    public Node prev;
}

public class Program
{
    public const int MAX_BYTEARRAYSIZE = 100000;
    public const int NODECOUNT = 50;

    public static int Main(string[] args)
    {
        int byteCount;
        int nodeCount = NODECOUNT;
        if (args.Length > 0)
        {
            byteCount = Convert.ToInt32(args[0]);

            if (byteCount < 0)
            {
                Console.WriteLine("Error! Invalid first argument");
                return -1;
            }

            if (args.Length > 1)
            {
                nodeCount = Convert.ToInt32(args[1]);

                if (nodeCount < 0)
                {
                    Console.WriteLine("Error! Invalid second argument");
                    return -2;
                }
            }

        }
        else
        {
            //find a random number for byteCount
            Random rand = new Random();
            byteCount = rand.Next(1, MAX_BYTEARRAYSIZE);
            Console.WriteLine("byte count = {0}; repro with {0}", byteCount);

        }


       
        Node head = new Node();
        MySizedReference sr = new MySizedReference(head);
        long listSize = sr.ApproximateSize;
        Console.WriteLine("Node base size = {0}", listSize);

        byte[] b = new byte[byteCount];
        head.data = b;
        listSize = sr.ApproximateSize;
        Console.WriteLine("Head size = {0}", listSize);

        long nodeSize = listSize;

        //add nodes to the list
        Console.WriteLine("Adding {0} nodes to the list", nodeCount);
        for (int i = 0; i < nodeCount; i++)
        {
            Node n;
            try
            {
                n = new Node(byteCount);
            }
            catch (OutOfMemoryException)
            {
                Console.WriteLine("OOM when allocating {0}th node", i);
                return 110;
            }
            Node.InsertToHead(head, n);

            long newSize = sr.ApproximateSize;
            Console.WriteLine("newSize = {0}", newSize);
            if (newSize != listSize + nodeSize)
            {
                Console.WriteLine("Error! Wrong list size");
                return 101;
            }
            listSize = newSize;
        }

        //add nodes to the list
        Console.WriteLine("Removing nodes from list");
        for (int i = 0; i < nodeCount; i++)
        {
            Node.Remove(head);

            long newSize = sr.ApproximateSize;
            Console.WriteLine("newSize = {0}", newSize);
            if (newSize != listSize - nodeSize)
            {
                Console.WriteLine("Error! Wrong list size after removing");
                return 102;
            }
            listSize = newSize;
        }

        //At the end there should be only one node left
        if (listSize != nodeSize)
        {
            Console.WriteLine("Error! Incorrect size");
            return 103;
        }

        Console.WriteLine("------------ Test passed ---------------");
        return 100;
    }

}