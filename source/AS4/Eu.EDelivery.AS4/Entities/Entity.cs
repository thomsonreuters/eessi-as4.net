using System;

namespace Eu.EDelivery.AS4.Entities
{
    public class Entity : IEquatable<Entity>, IEntity
    {
        public long Id { get; private set; }

        public DateTimeOffset InsertionTime { get; set; }

        public DateTimeOffset ModificationTime { get; set; }

        public void InitializeIdFromDatabase(long id)
        {
            Id = id;
        }

        public bool IsTransient => Id == default(long);

        /// <summary>
        /// Update the <see cref="Entity"/> to lock it with a given <paramref name="value"/>.
        /// </summary>
        /// <param name="value">Value indicating the <see cref="Entity"/> is locked.</param>
        public virtual void Lock(string value) { }

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(Entity other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            
            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Id == other.Id;
        }

        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        /// <param name="obj">The object to compare with the current object. </param>
        public override bool Equals(object obj)
        {
            var other = obj as Entity;

            if (other == null)
            {
                return false;
            }
            
            if (other.GetType() != this.GetType())
            {
                return false;
            }

            return Equals(other);
        }

        private int? _hashCode;

        /// <summary>Serves as the default hash function. </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {            
            // ReSharper disable NonReadonlyMemberInGetHashCode
            if (_hashCode.HasValue)                
            {
                return _hashCode.Value;
            }

            _hashCode = IsTransient ? base.GetHashCode() : Id.GetHashCode();
            
            return _hashCode.Value;
            // ReSharper restore NonReadonlyMemberInGetHashCode
        }
    }
}