 namespace RoadRegistry.BackOffice.Core
 {
     using System;
     using System.Collections.Generic;

     public class ProblemParameter : IEquatable<ProblemParameter>, IEqualityComparer<ProblemParameter>
     {
         public ProblemParameter(string name, string value)
         {
             Name = name ?? throw new ArgumentNullException(nameof(name));
             Value = value ?? throw new ArgumentNullException(nameof(value));
         }

         public string Name { get; }

         public string Value { get; }

         public virtual bool Equals(ProblemParameter other) => other != null
            && string.Equals(Name, other.Name)
            && string.Equals(Value, other.Value);

         public override bool Equals(object obj) => obj is ProblemParameter other && Equals(other);
         public override int GetHashCode() => Name.GetHashCode() ^ Value.GetHashCode();

         public Messages.ProblemParameter Translate() => new Messages.ProblemParameter
         {
             Name = Name, Value = Value
         };

         public bool Equals(ProblemParameter x, ProblemParameter y)
         {
             if (ReferenceEquals(x, y))
             {
                 return true;
             }

             if (x is null)
             {
                 return false;
             }

             if (y is null)
             {
                 return false;
             }

             if (x.GetType() != y.GetType())
             {
                 return false;
             }

             return x.Name == y.Name
                 && x.Value == y.Value;
         }

         public int GetHashCode(ProblemParameter obj)
         {
             return HashCode.Combine(obj.Name, obj.Value);
         }
     }
 }
