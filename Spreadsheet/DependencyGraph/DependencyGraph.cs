// Skeleton implementation written by Joe Zachary for CS 3500, September 2013.
// Version 1.1 (Fixed error in comment for RemoveDependency.)
// Version 1.2 - Daniel Kopta 
//               (Clarified meaning of dependent and dependee.)
//               (Clarified names in solution/project structure.)
//
// Implementation of methods written by Josie Fiedel, September 9, 2022.

namespace SpreadsheetUtilities
{
    /// <summary>
    /// (s1,t1) is an ordered pair of strings
    /// t1 depends on s1; s1 must be evaluated before t1
    /// 
    /// A DependencyGraph can be modeled as a set of ordered pairs of strings.  Two ordered pairs
    /// (s1,t1) and (s2,t2) are considered equal if and only if s1 equals s2 and t1 equals t2.
    /// Recall that sets never contain duplicates.  If an attempt is made to add an element to a 
    /// set, and the element is already in the set, the set remains unchanged.
    /// 
    /// Given a DependencyGraph DG:
    /// 
    ///    (1) If s is a string, the set of all strings t such that (s,t) is in DG is called dependents(s).
    ///        (The set of things that depend on s)    
    ///        
    ///    (2) If s is a string, the set of all strings t such that (t,s) is in DG is called dependees(s).
    ///        (The set of things that s depends on) 
    //
    // For example, suppose DG = {("a", "b"), ("a", "c"), ("b", "d"), ("d", "d")}
    //     dependents("a") = {"b", "c"}
    //     dependents("b") = {"d"}
    //     dependents("c") = {}
    //     dependents("d") = {"d"}
    //     dependees("a") = {}
    //     dependees("b") = {"a"}
    //     dependees("c") = {"a"}
    //     dependees("d") = {"b", "d"}
    /// </summary>
    public class DependencyGraph
    {
        // dependees: The values are the key's set of dependees.
        // dependents: The values are the key's set of dependents.
        private Dictionary<string, HashSet<string>> dependees, dependents;
        private int size;

        /// <summary>
        /// Creates an empty DependencyGraph.
        /// </summary>
        public DependencyGraph()
        {
            dependees = new();
            dependents = new();
            size = 0;
        }

        /// <summary>
        /// The number of ordered pairs in the DependencyGraph.
        /// </summary>
        public int Size
        {
            get { return size; }
        }

        /// <summary>
        /// The size of dependees(s).
        /// This property is an example of an indexer.  If dg is a DependencyGraph, you would
        /// invoke it like this:
        /// dg["a"]
        /// It should return the size of dependees("a")
        /// </summary>
        public int this[string s]
        {
            get 
            {
                if (!dependees.ContainsKey(s))
                    return 0;
                return dependees[s].Count;
            }
        }

        /// <summary>
        /// Reports whether dependents(s) is non-empty.
        /// </summary>
        public bool HasDependents(string s)
        {
            return dependents.ContainsKey(s);
        }

        /// <summary>
        /// Reports whether dependees(s) is non-empty.
        /// </summary>
        public bool HasDependees(string s)
        {
            return dependees.ContainsKey(s);
        }

        /// <summary>
        /// Enumerates dependents(s).
        /// </summary>
        public IEnumerable<string> GetDependents(string s)
        { 
            if(!dependents.TryGetValue(s, out HashSet<string>? result))
                return new HashSet<string>();   // Empty HashSet if s has no dependents.
            return new HashSet<string>(result);
        }

        /// <summary>
        /// Enumerates dependees(s).
        /// </summary>
        public IEnumerable<string> GetDependees(string s)
        {
            if (!dependees.TryGetValue(s, out HashSet<string>? result))
                return new HashSet<string>();   // Empty HashSet if s has no dependees.
            return new HashSet<string>(result);
        }
    

        /// <summary>
        /// Helper method for the AddDependency method. While the AddDependency method edits the
        /// dependees dictionary, this method edits the dependents dictionary. 
        /// </summary>
        /// <param name="s"> The dependee of t </param>
        /// <param name="t"> The dependent of s </param>
        private void ModifyDependents(string s, string t)
        {
            if (!dependents.ContainsKey(s))
                // Add the new dependee (s) and a new set of its dependents, containing t.
                dependents.Add(s, new HashSet<string>() { t });
            else
                // Modify the dependents set of s to include t.
                dependents[s].Add(t);
            size++;
        }

        /// <summary>
        /// <para>Adds the ordered pair (s,t), if it doesn't exist</para>
        /// 
        /// <para>This should be thought of as:</para>   
        /// 
        ///   t depends on s
        ///
        /// </summary>
        /// <param name="s"> s must be evaluated first. T depends on S</param>
        /// <param name="t"> t cannot be evaluated until s is</param>        /// 
        public void AddDependency(string s, string t)
        {
            // If t does not exist in the dependees dictionary, add it along with a new set containing s.
            // Then, modify the dependees dictionary to include t in the dependents set of s.
            if (!dependees.ContainsKey(t))
            {
                dependees.Add(t, new HashSet<string>() { s });
                ModifyDependents(s, t);
            }
            else
            {
                // Check if the dependees set of t includes s. If not, modify the set to include s.
                if (!dependees[t].Contains(s))
                {
                    dependees[t].Add(s);
                    ModifyDependents(s, t);
                }
            }
        }

        /// <summary>
        /// Removes the ordered pair (s,t), if it exists
        /// </summary>
        /// <param name="s"></param>
        /// <param name="t"></param>
        public void RemoveDependency(string s, string t)
        {
            // If t is a dependent of s, remove it from the dependent set.
            if(dependents.ContainsKey(s) && dependents[s].Contains(t))
            {
                dependents[s].Remove(t);
                // If s is a dependee of t, remove it from the dependee set.
                if (dependees[t].Contains(s))
                    dependees[t].Remove(s);
                size--;

                // If s has no dependents, it can be removed from the dg.
                if (dependents[s].Count == 0)
                    dependents.Remove(s);

                // If t has no dependents, it can be removed from the dg.
                if (dependees[t].Count == 0)
                    dependees.Remove(t);
            }
        }

        /// <summary>
        /// Removes all existing ordered pairs of the form (s,r).  Then, for each
        /// t in newDependents, adds the ordered pair (s,t).
        /// </summary>
        public void ReplaceDependents(string s, IEnumerable<string> newDependents)
        {
            // Must exist for it to be removed.
            if (dependents.ContainsKey(s))
                foreach(string t in dependents[s])
                    RemoveDependency(s, t);

            // If the key/value does not exist, then the dependency is added.
            foreach (string t in newDependents)
                if(!dependents.ContainsKey(s) || !dependents[s].Contains(t))
                    AddDependency(s, t);
        }

        /// <summary>
        /// Removes all existing ordered pairs of the form (r,s).  Then, for each 
        /// t in newDependees, adds the ordered pair (t,s).
        /// </summary>
        public void ReplaceDependees(string s, IEnumerable<string> newDependees)
        {
            // Must exist for it to be removed.
            if (dependees.ContainsKey(s))
                foreach (string t in dependees[s])
                    RemoveDependency(t, s);

            // If the key/value does not exist, then the dependency is added.
            foreach (string t in newDependees)
                if (!dependees.ContainsKey(s) || !dependees[s].Contains(t))
                    AddDependency(t, s);
        }
    }
}
