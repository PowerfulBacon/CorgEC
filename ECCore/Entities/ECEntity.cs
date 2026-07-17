using ECCore.Components;

using ECCore.Signals;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ECCore.Entities
{

	/// <summary>
	/// The base of logical entities in the game world,
	/// seperated from their representation in Unity.
	/// </summary>
	/// <typeparam name="TParentType">Should be the type of the class that inherits this type.</typeparam>
	public abstract class ECEntity<TParentType> : SignalHolder
		where TParentType : ECEntity<TParentType>
	{

		public bool Initialized { get; private set; } = false;
		public bool Destroyed { get; private set; } = false;

		public void Initialise()
		{
			ValidateComponents();
			InitialiseComponents();
			Initialized = true;
		}

		public void Destroy()
		{
			// Remove all our components
			RemoveComponents();
			// Destroy the entity
			Destroyed = true;
		}

		#region Components

		private List<Component<TParentType>> components = new List<Component<TParentType>>();

		/// <summary>
		/// The components, implemented as a list as we shouldn't need to get components
		/// a lot as the dependency system handles caching them for direct access.
		/// Use the [Dependency] attribute if you need to frequently interface with another specific
		/// component.
		/// </summary>
		public IReadOnlyList<Component<TParentType>> Components => components;

		public bool HasComponent<T>()
			where T : Component<TParentType>
		{
			return components
				.Where(x => x.GetType() == typeof(T))
				.Any();
		}

#if NET6_0_OR_GREATER
		public T? GetComponent<T>()
#else
		public T GetComponent<T>()
#endif
			where T : Component<TParentType>
		{
			if (Destroyed)
			{
				throw new NullReferenceException("Attempting to access a destroyed entity.");
			}

			return (T)components
				.Where(x => x.GetType() == typeof(T))
				.FirstOrDefault();
		}

		public T GetOrAddComponent<T>(Func<T> componentCreator)
			where T : Component<TParentType>
		{
			if (Destroyed)
			{
				throw new NullReferenceException("Attempting to access a destroyed entity.");
			}

			var result = components
				.Where(x => x.GetType() == typeof(T))
				.SingleOrDefault() as T;
			if (result != default)
			{
				return result;
			}

			T createdComponent = componentCreator();
			if (TryAddComponent(createdComponent))
			{
				return createdComponent;
			}

			throw new Exception("Could not add the created component to the entity.");
		}

#if NET6_0_OR_GREATER
	public bool TryGetComponent<T>(out T? component)
#else
		public bool TryGetComponent<T>(out T component)
#endif
			where T : Component<TParentType>
		{
			if (Destroyed)
			{
				throw new NullReferenceException("Attempting to access a destroyed entity.");
			}

			component = components
				.Where(x => x.GetType() == typeof(T))
				.SingleOrDefault() as T;
			return component != default;
		}

		public Component<TParentType> GetComponent(Type componentType)
		{
			if (Destroyed)
			{
				throw new NullReferenceException("Attempting to access a destroyed entity.");
			}

			return components
				.Where(x => x.GetType() == componentType)
				.Single();
		}

#if NET6_0_OR_GREATER
	public bool TryGetComponent(Type componentType, out Component<TParentType>? component, bool allowSubtypes = true)
	{
		if (Destroyed)
			throw new NullReferenceException("Attempting to access a destroyed entity.");
		component = components
			.Where(x => allowSubtypes ? componentType.IsAssignableFrom(x.GetType()) : x.GetType() == componentType)
			.SingleOrDefault();
		return component != default;
	}
#else
		public bool TryGetComponent(Type componentType, out Component<TParentType> component, bool allowSubtypes = true)
		{
			if (Destroyed)
			{
				throw new NullReferenceException("Attempting to access a destroyed entity.");
			}

			component = components
				.Where(x => allowSubtypes ? componentType.IsAssignableFrom(x.GetType()) : x.GetType() == componentType)
				.SingleOrDefault();
			return component != default;
		}
#endif

		/// <summary>
		/// Try to add a component to this entity, returns false if the component could not
		/// be added due to duplication rules.
		/// </summary>
		/// <param name="component"></param>
		/// <returns>Returns true if the component was added to the entity.</returns>
		public bool TryAddComponent(Component<TParentType> component)
		{
			if (Destroyed)
			{
				throw new NullReferenceException("Attempting to access a destroyed entity.");
			}

			if (Initialized)
			{
				try
				{
					component.Parent = (TParentType)this;
					component.SetupDependencies();
					component.Initialise();
					components.Add(component);
					return true;
				}
				catch
				{
					return false;
				}
			}
			else
			{
				component.Parent = (TParentType)this;
				components.Add(component);
				return true;
			}
		}

		/// <summary>
		/// Removes a component from an entity, note that this will also unregister
		/// all signals from the component.
		/// </summary>
		/// <param name="component"></param>
		/// <returns></returns>
		/// <exception cref="NullReferenceException"></exception>
		public bool RemoveComponent(Component<TParentType> component)
		{
			if (Destroyed)
			{
				throw new NullReferenceException("Attempting to access a destroyed entity.");
			}

			components.Remove(component);
			component.Remove();
			return true;
		}

		/// <summary>
		/// Validates that the components added have the dependencies that they require
		/// Returns false if failed.
		/// If we do not have that component, a warning will be produced and that
		/// component will be added.
		/// </summary>
		/// <returns></returns>
		internal bool ValidateComponents()
		{
			return Components
				.All(component => component.SetupDependencies());
		}

		internal void InitialiseComponents()
		{
			foreach (Component<TParentType> component in Components)
			{
				component.Initialise();
			}
		}

		internal void RemoveComponents()
		{
			var componentList = Components.ToList();
			components.Clear();
			foreach (Component<TParentType> component in componentList)
			{
				component.Remove();
			}
		}

		#endregion

		#region Operators

		public static bool operator true(ECEntity<TParentType> entity)
		{
			return entity != null && !entity.Destroyed;
		}

		public static bool operator false(ECEntity<TParentType> entity)
		{
			return entity == null || entity.Destroyed;
		}

		public static bool operator !(ECEntity<TParentType> entity)
		{
			return entity == null || entity.Destroyed;
		}

		public static bool operator &(ECEntity<TParentType> entity, bool other)
		{
			if (!entity)
			{
				return false;
			}

			return other;
		}

#if NET6_0_OR_GREATER
	public static bool operator ==(ECEntity<TParentType>? entity, object? other)
	{
		if (entity is null)
		{
			if (other is ECEntity<TParentType> otherEntity)
				return otherEntity.Destroyed;
			return other is null;
		}
		if (other is not ECEntity<TParentType>)
		{
			if (other is null && entity.Destroyed)
				return true;
			return false;
		}
		if (other is null)
			return entity.Destroyed;
		if (entity.Destroyed && ((ECEntity<TParentType>)other).Destroyed)
			return true;
		return entity.Equals(other);
	}

	public static bool operator !=(ECEntity<TParentType>? entity, object? other)
	{
		return !(entity == other);
	}

	public override bool Equals(object? obj)
	{
		return base.Equals(obj);
	}
#else
		public static bool operator ==(ECEntity<TParentType> entity, object other)
		{
			if (entity is null)
			{
				if (other is ECEntity<TParentType> otherEntity)
				{
					return otherEntity.Destroyed;
				}

				return other is null;
			}
			if (!(other is ECEntity<TParentType>))
			{
				if (other is null && entity.Destroyed)
				{
					return true;
				}

				return false;
			}
			if (other is null)
			{
				return entity.Destroyed;
			}

			if (entity.Destroyed && ((ECEntity<TParentType>)other).Destroyed)
			{
				return true;
			}

			return entity.Equals(other);
		}

		public static bool operator !=(ECEntity<TParentType> entity, object other)
		{
			return !(entity == other);
		}

		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}
#endif

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		#endregion

	}

}