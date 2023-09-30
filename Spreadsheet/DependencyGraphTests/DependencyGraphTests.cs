using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SpreadsheetUtilities;


namespace DevelopmentTests
{
    /// <summary>
    ///This is a test class for DependencyGraphTest and is intended
    ///to contain all DependencyGraphTest Unit Tests
    ///</summary>
    [TestClass()]
    public class DependencyGraphTest
    {

        /// <summary>
        ///Empty graph should contain nothing
        ///</summary>
        [TestMethod()]
        public void SimpleEmptyTest()
        {
            DependencyGraph t = new DependencyGraph();
            Assert.AreEqual(0, t.Size);
        }


        /// <summary>
        ///Empty graph should contain nothing
        ///</summary>
        [TestMethod()]
        public void SimpleEmptyRemoveTest()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("x", "y");
            Assert.AreEqual(1, t.Size);
            t.RemoveDependency("x", "y");
            Assert.AreEqual(0, t.Size);
        }


        /// <summary>
        ///Empty graph should contain nothing
        ///</summary>
        [TestMethod()]
        public void EmptyEnumeratorTest()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("x", "y");
            IEnumerator<string> e1 = t.GetDependees("y").GetEnumerator();
            Assert.IsTrue(e1.MoveNext());
            Assert.AreEqual("x", e1.Current);
            IEnumerator<string> e2 = t.GetDependents("x").GetEnumerator();
            Assert.IsTrue(e2.MoveNext());
            Assert.AreEqual("y", e2.Current);
            t.RemoveDependency("x", "y");
            Assert.IsFalse(t.GetDependees("y").GetEnumerator().MoveNext());
            Assert.IsFalse(t.GetDependents("x").GetEnumerator().MoveNext());
        }


        /// <summary>
        ///Replace on an empty DG shouldn't fail
        ///</summary>
        [TestMethod()]
        public void SimpleReplaceTest()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("x", "y");
            Assert.AreEqual(t.Size, 1);
            t.RemoveDependency("x", "y");
            t.ReplaceDependents("x", new HashSet<string>());
            t.ReplaceDependees("y", new HashSet<string>());
        }



        ///<summary>
        ///It should be possibe to have more than one DG at a time.
        ///</summary>
        [TestMethod()]
        public void StaticTest()
        {
            DependencyGraph t1 = new DependencyGraph();
            DependencyGraph t2 = new DependencyGraph();
            t1.AddDependency("x", "y");
            Assert.AreEqual(1, t1.Size);
            Assert.AreEqual(0, t2.Size);
        }




        /// <summary>
        ///Non-empty graph contains something
        ///</summary>
        [TestMethod()]
        public void SizeTest()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("a", "b");
            t.AddDependency("a", "c");
            t.AddDependency("c", "b");
            t.AddDependency("b", "d");
            Assert.AreEqual(4, t.Size);
        }


        /// <summary>
        ///Non-empty graph contains something
        ///</summary>
        [TestMethod()]
        public void EnumeratorTest()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("a", "b");
            t.AddDependency("a", "c");
            t.AddDependency("c", "b");
            t.AddDependency("b", "d");

            IEnumerator<string> e = t.GetDependees("a").GetEnumerator();
            Assert.IsFalse(e.MoveNext());

            e = t.GetDependees("b").GetEnumerator();
            Assert.IsTrue(e.MoveNext());
            String s1 = e.Current;
            Assert.IsTrue(e.MoveNext());
            String s2 = e.Current;
            Assert.IsFalse(e.MoveNext());
            Assert.IsTrue(((s1 == "a") && (s2 == "c")) || ((s1 == "c") && (s2 == "a")));

            e = t.GetDependees("c").GetEnumerator();
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("a", e.Current);
            Assert.IsFalse(e.MoveNext());

            e = t.GetDependees("d").GetEnumerator();
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("b", e.Current);
            Assert.IsFalse(e.MoveNext());
        }




        /// <summary>
        ///Non-empty graph contains something
        ///</summary>
        [TestMethod()]
        public void ReplaceThenEnumerate()
        {
            DependencyGraph t = new DependencyGraph();
            t.AddDependency("x", "b");
            t.AddDependency("a", "z");
            t.ReplaceDependents("b", new HashSet<string>());
            t.AddDependency("y", "b");
            t.ReplaceDependents("a", new HashSet<string>() { "c" });
            t.AddDependency("w", "d");
            t.ReplaceDependees("b", new HashSet<string>() { "a", "c" });
            t.ReplaceDependees("d", new HashSet<string>() { "b" });

            IEnumerator<string> e = t.GetDependees("a").GetEnumerator();
            Assert.IsFalse(e.MoveNext());

            e = t.GetDependees("b").GetEnumerator();
            Assert.IsTrue(e.MoveNext());
            String s1 = e.Current;
            Assert.IsTrue(e.MoveNext());
            String s2 = e.Current;
            Assert.IsFalse(e.MoveNext());
            Assert.IsTrue(((s1 == "a") && (s2 == "c")) || ((s1 == "c") && (s2 == "a")));

            e = t.GetDependees("c").GetEnumerator();
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("a", e.Current);
            Assert.IsFalse(e.MoveNext());

            e = t.GetDependees("d").GetEnumerator();
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("b", e.Current);
            Assert.IsFalse(e.MoveNext());
        }



        /// <summary>
        ///Using lots of data
        ///</summary>
        [TestMethod()]
        public void StressTest()
        {
            // Dependency graph
            DependencyGraph t = new DependencyGraph();

            // A bunch of strings to use
            const int SIZE = 200;
            string[] letters = new string[SIZE];
            for (int i = 0; i < SIZE; i++)
            {
                letters[i] = ("" + (char)('a' + i));
            }

            // The correct answers
            HashSet<string>[] dents = new HashSet<string>[SIZE];
            HashSet<string>[] dees = new HashSet<string>[SIZE];
            for (int i = 0; i < SIZE; i++)
            {
                dents[i] = new HashSet<string>();
                dees[i] = new HashSet<string>();
            }

            // Add a bunch of dependencies
            for (int i = 0; i < SIZE; i++)
            {
                for (int j = i + 1; j < SIZE; j++)
                {
                    t.AddDependency(letters[i], letters[j]);
                    dents[i].Add(letters[j]);
                    dees[j].Add(letters[i]);
                }
            }

            // Remove a bunch of dependencies
            for (int i = 0; i < SIZE; i++)
            {
                for (int j = i + 4; j < SIZE; j += 4)
                {
                    t.RemoveDependency(letters[i], letters[j]);
                    dents[i].Remove(letters[j]);
                    dees[j].Remove(letters[i]);
                }
            }

            // Add some back
            for (int i = 0; i < SIZE; i++)
            {
                for (int j = i + 1; j < SIZE; j += 2)
                {
                    t.AddDependency(letters[i], letters[j]);
                    dents[i].Add(letters[j]);
                    dees[j].Add(letters[i]);
                }
            }

            // Remove some more
            for (int i = 0; i < SIZE; i += 2)
            {
                for (int j = i + 3; j < SIZE; j += 3)
                {
                    t.RemoveDependency(letters[i], letters[j]);
                    dents[i].Remove(letters[j]);
                    dees[j].Remove(letters[i]);
                }
            }

            // Make sure everything is right
            for (int i = 0; i < SIZE; i++)
            {
                Assert.IsTrue(dents[i].SetEquals(new HashSet<string>(t.GetDependents(letters[i]))));
                Assert.IsTrue(dees[i].SetEquals(new HashSet<string>(t.GetDependees(letters[i]))));
            }
        }



        // PERSONAL TESTS: -------------------------------------------------------------------------------

        [TestMethod()]
        public void AddDependenciesCheckSize()
        {
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("a", "b");
            graph.AddDependency("b", "c");
            graph.AddDependency("c", "d");
            graph.AddDependency("d", "e");
            graph.AddDependency("e", "f");
            graph.AddDependency("f", "a");

            Assert.AreEqual(6, graph.Size);
        }

        [TestMethod()]
        public void ReplaceDependeesEmpty()
        {
            DependencyGraph graph = new DependencyGraph();
            graph.ReplaceDependees("a", new HashSet<string>() { "b", "c" });
            Assert.AreEqual(2, graph["a"]);
            Assert.AreEqual(0, graph["b"]);
            Assert.AreEqual(0, graph["c"]);
        }

        [TestMethod()]
        public void ReplaceDependentsEmpty()
        {
            DependencyGraph graph = new DependencyGraph();
            graph.ReplaceDependents("a", new HashSet<string>() { "b", "c" });
            Assert.AreEqual(0, graph["a"]);
            Assert.AreEqual(1, graph["b"]);
            Assert.AreEqual(1, graph["c"]);
        }

        [TestMethod()]
        public void ReplaceDependenciesCheckSize()
        {
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("a", "b");
            graph.AddDependency("b", "c");
            graph.AddDependency("c", "d");
            graph.AddDependency("d", "e");
            graph.AddDependency("e", "f");
            graph.AddDependency("f", "a");

            HashSet<string> set = new HashSet<string>();
            set.Add("b");
            set.Add("c");
            set.Add("d");

            graph.ReplaceDependees("a", set);

            Assert.AreEqual(8, graph.Size);
        }

        [TestMethod()]
        public void ReplaceDependenciesCheckSize2()
        {
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("a", "b");
            graph.AddDependency("b", "c");
            graph.AddDependency("c", "d");
            graph.AddDependency("d", "e");
            graph.AddDependency("e", "f");
            graph.AddDependency("f", "a");

            HashSet<string> set = new HashSet<string>();
            set.Add("b");
            set.Add("c");
            set.Add("d");

            graph.ReplaceDependees("a", set);

            HashSet<string> set2 = new HashSet<string>();
            set2.Add("b");
            set2.Add("b");
            set2.Add("c");
            set2.Add("d");

            graph.ReplaceDependees("a", set2);

            Assert.AreEqual(8, graph.Size);
        }

        [TestMethod()]
        public void AddDuplicatesCheckSize()
        {
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("a", "b");
            graph.AddDependency("a", "b");
            graph.AddDependency("a", "b");

            Assert.AreEqual(1, graph.Size);

        }

        [TestMethod()]
        public void AddSameKeyValuePair()
        {
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("a", "a");
            Assert.AreEqual(1, graph["a"]);
            Assert.IsTrue(graph.HasDependents("a"));
            Assert.IsTrue(graph.HasDependees("a"));

            graph.AddDependency("a", "a");
            graph.ReplaceDependents("a", new HashSet<string>() { "a" });
            Assert.AreEqual(1, graph.Size);
            Assert.AreEqual(1, graph["a"]);
            Assert.IsTrue(graph.HasDependents("a"));
            Assert.IsTrue(graph.HasDependees("a"));
        }

        [TestMethod()]
        public void AddDuplicatesRemoveCheckSize2()
        {
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("a", "b");
            graph.AddDependency("a", "b");
            graph.AddDependency("a", "b");

            graph.RemoveDependency("a", "b");
            Assert.AreEqual(0, graph.Size);

            graph.AddDependency("a", "b");
            graph.AddDependency("a", "c");
            graph.AddDependency("c", "a");
            Assert.AreEqual(3, graph.Size);

        }

        [TestMethod()]
        public void AddAndRemoveCheckSize()
        {
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("a", "b");
            graph.AddDependency("b", "c");
            graph.AddDependency("a", "b");
            Assert.AreEqual(2, graph.Size);

            graph.RemoveDependency("a", "b");
            graph.RemoveDependency("b", "c");
            graph.RemoveDependency("a", "b");
            graph.RemoveDependency("a", "b");
            Assert.AreEqual(0, graph.Size);

            graph.AddDependency("a", "b");
            graph.AddDependency("b", "c");
            graph.AddDependency("c", "b");
            Assert.AreEqual(3, graph.Size);
        }

        [TestMethod()]
        public void ReplaceDependencyCheckSize()
        {
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("a", "b");

            HashSet<string> set = new HashSet<string>();
            set.Add("b");
            set.Add("c");
            set.Add("d");

            graph.ReplaceDependents("a", set);
            graph.ReplaceDependents("a", set);

            Assert.AreEqual(3, graph.Size);
        }

        [TestMethod()]
        public void ReplaceDependencyCheckSize2()
        {
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("a", "b");

            HashSet<string> set = new HashSet<string>();
            set.Add("b");
            set.Add("c");
            set.Add("d");

            graph.ReplaceDependents("a", set);
            graph.ReplaceDependees("b", new HashSet<string> { "a" });
            graph.ReplaceDependees("c", new HashSet<string> { "a" });
            graph.ReplaceDependees("d", new HashSet<string> { "a" });

            Assert.AreEqual(3, graph.Size);
        }

        [TestMethod()]
        public void DependeesCount()
        {
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("a", "b");
            Assert.AreEqual(1, graph["b"]);

            HashSet<string> set = new HashSet<string>();
            set.Add("b");
            set.Add("c");
            set.Add("d");

            graph.ReplaceDependents("a", set);
            graph.ReplaceDependees("b", new HashSet<string> { "a" });
            graph.ReplaceDependees("c", new HashSet<string> { "a" });
            graph.ReplaceDependees("d", new HashSet<string> { "a" });

            Assert.AreEqual(0, graph["a"]);
        }

        [TestMethod()]
        public void DependeesCount2()
        {
            DependencyGraph graph = new DependencyGraph();
            Assert.AreEqual(0, graph["a"]);

            graph.AddDependency("a", "b");
            Assert.AreEqual(0, graph["a"]);
            Assert.AreEqual(1, graph["b"]);

            graph.ReplaceDependees("a", new HashSet<string>() { "b" });
            graph.ReplaceDependees("a", new HashSet<string>() { "b" });
            graph.ReplaceDependees("b", new HashSet<string>() { "a" });
            Assert.AreEqual(1, graph["a"]);
            Assert.AreEqual(1, graph["b"]);
        }

        [TestMethod()]
        public void HasDependentsRemove1()
        {
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("a", "b");
            graph.AddDependency("a", "c");
            graph.AddDependency("a", "b");
            graph.AddDependency("a", "d");

            Assert.IsTrue(graph.HasDependents("a"));
            Assert.IsFalse(graph.HasDependents("b"));

            graph.RemoveDependency("a", "b");
            graph.RemoveDependency("a", "c");
            graph.RemoveDependency("a", "d");

            Assert.IsFalse(graph.HasDependents("a"));
            Assert.IsFalse(graph.HasDependents("b"));
        }

        [TestMethod()]
        public void HasDependeesRemove1()
        {
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("a", "b");
            graph.AddDependency("a", "c");
            graph.AddDependency("a", "b");
            graph.AddDependency("a", "d");

            Assert.IsFalse(graph.HasDependees("a"));
            Assert.IsTrue(graph.HasDependees("b"));

            graph.RemoveDependency("a", "b");
            graph.RemoveDependency("a", "c");
            graph.RemoveDependency("a", "d");

            Assert.IsFalse(graph.HasDependees("a"));
            Assert.IsFalse(graph.HasDependees("b"));
        }

        [TestMethod()]
        public void SizeAfterRemoval()
        {
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("a", "b");
            graph.RemoveDependency("a", "b");
            Assert.AreEqual(0, graph["a"]);
        }

        [TestMethod()]
        public void ReplaceSameDependees()
        {
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("a", "b");
            Assert.AreEqual(0, graph["a"]);
            Assert.AreEqual(1, graph["b"]);
            graph.ReplaceDependees("b", new HashSet<string>() { "a" });
            Assert.AreEqual(0, graph["a"]);
            Assert.AreEqual(1, graph["b"]);
        }

        [TestMethod()]
        public void DependeeCycleReplaceAndRemove()
        {
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("a", "b");
            graph.ReplaceDependees("a", new HashSet<string>() { "b" });
            Assert.AreEqual(1, graph["a"]);
            Assert.AreEqual(1, graph["b"]);
            Assert.AreEqual(2, graph.Size);

            graph.RemoveDependency("a", "b");
            Assert.AreEqual(1, graph["a"]);
            Assert.AreEqual(0, graph["b"]);
        }

        [TestMethod()]
        public void DependeeCycleReplaceAndRemove2()
        {
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("a", "b");
            graph.ReplaceDependees("a", new HashSet<string>() { "b" });
            graph.ReplaceDependees("a", new HashSet<string>() { "b" });

            Assert.AreEqual(1, graph["a"]);
            Assert.AreEqual(1, graph["b"]);
            Assert.AreEqual(2, graph.Size);

            graph.RemoveDependency("a", "b");
            Assert.AreEqual(1, graph["a"]);
            Assert.AreEqual(0, graph["b"]);
        }

        [TestMethod()]
        public void DependentCycleReplaceAndRemove()
        {
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("a", "b");
            graph.ReplaceDependents("b", new HashSet<string>() { "a" });
            Assert.AreEqual(1, graph["a"]);
            Assert.AreEqual(1, graph["b"]);
            Assert.AreEqual(2, graph.Size);

            graph.RemoveDependency("a", "b");
            Assert.AreEqual(1, graph["a"]);
            Assert.AreEqual(0, graph["b"]);
        }

        [TestMethod()]
        public void DependentCycleReplaceAndRemove2()
        {
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("a", "b");
            graph.ReplaceDependents("b", new HashSet<string>() { "a" });
            graph.ReplaceDependents("b", new HashSet<string>() { "a" });
            Assert.AreEqual(1, graph["a"]);
            Assert.AreEqual(1, graph["b"]);
            Assert.AreEqual(2, graph.Size);

            graph.RemoveDependency("a", "b");
            Assert.AreEqual(1, graph["a"]);
            Assert.AreEqual(0, graph["b"]);
        }

        [TestMethod()]
        public void MassMethodUse()
        {
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("a", "b");
            graph.AddDependency("a", "c");
            graph.AddDependency("a", "d");
            graph.AddDependency("b", "b");
            graph.AddDependency("d", "d");
            graph.AddDependency("c", "a");
            Assert.AreEqual(6, graph.Size);
            graph.ReplaceDependents("b", new HashSet<string>() { "a" });
            graph.ReplaceDependents("b", new HashSet<string>() { "a" });
            Assert.AreEqual(6, graph.Size);
            Assert.AreEqual(2, graph["a"]);
            Assert.AreEqual(1, graph["b"]);
        }

        [TestMethod()]
        public void EmptyGraphIEnumerable()
        {
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("a", "b");
            graph.RemoveDependency("a", "b");

            IEnumerator<string> e = graph.GetDependees("a").GetEnumerator();
            Assert.IsFalse(e.MoveNext());
        }

        [TestMethod()]
        public void AddReplaceRemoveIEnumerable()
        {
            DependencyGraph graph = new DependencyGraph();
            graph.AddDependency("a", "b");
            graph.AddDependency("b", "c");
            graph.AddDependency("c", "b");
            graph.AddDependency("f", "a");
            graph.ReplaceDependees("a", new HashSet<string>() { "d", "e" });
            graph.RemoveDependency("a", "b");

            IEnumerator<string> e = graph.GetDependees("a").GetEnumerator();
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("d", e.Current);
            Assert.IsTrue(e.MoveNext());
            Assert.AreEqual("e", e.Current);
            Assert.IsFalse(e.MoveNext());

            IEnumerator<string> f = graph.GetDependents("a").GetEnumerator();
            Assert.IsFalse(f.MoveNext());
        }
    }
}