using ECCore.Attributes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ECCore.Components
{
	public abstract partial class Component<TParent>
	{

		/// <summary>
		/// Cache of the component type to the dependency.
		/// </summary>
		private static ConcurrentDictionary<Type, IEnumerable<(DependencyAttribute attribute, ISharedDependency dependency)>> DependencyCache
			= new ConcurrentDictionary<Type, IEnumerable<(DependencyAttribute attribute, ISharedDependency dependency)>>();

#if NET6_0_OR_GREATER
	/// <summary>
	/// Sets up the dependencies for this component, must be called before
	/// the component is ready to use.
	/// Dependencies allow for direct function calls between 2 different components,
	/// for cases where you may need exactly 1 call, rather than the 0 to many calls
	/// that signals offer.
	/// </summary>
	/// <returns></returns>
	internal bool SetupDependencies()
	{
		if (Parent == null)
			throw new InvalidOperationException("Cannot setup dependencies on a component that has no parent.");
		// Try to access the dependency cache, or build it
		if (!DependencyCache.TryGetValue(GetType(), out var dependencies))
		{
			dependencies = GetType()
				.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
				.Where(x => x.GetCustomAttribute<DependencyAttribute>() != null && typeof(Component<TParent>).IsAssignableFrom(x.FieldType))
				.Select(x => (x.GetCustomAttribute<DependencyAttribute>()!, (ISharedDependency)new FieldDependency(x)))
				.Union(
					GetType()
						.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
						.Where(x => x.GetCustomAttribute<DependencyAttribute>() != null && typeof(Component<TParent>).IsAssignableFrom(x.PropertyType))
						.Select(x => (x.GetCustomAttribute<DependencyAttribute>()!, (ISharedDependency)new PropertyDependency(x)))
				);
			DependencyCache.TryAdd(GetType(), dependencies);
		}
		bool valid = true;
		// Now that we have our dependencies, set them up
		foreach (var dep in dependencies)
		{
			var dependency = dep.dependency;
            // Try to locate the component, or error
            if (Parent.TryGetComponent(dependency.dependencyType, out var located))
			{
				dependency.Set(this, located!);
			}
			else if (!(dep.attribute.Optional && dep.dependency.Nullable))
			{
				// Failed to fetch the required dependency, throw a warning and add it
				valid = false;
				// We might fail to add the component, in which case I have no idea what to do
				Component<TParent>? createdComponent = Activator.CreateInstance(dependency.dependencyType, new object[] { Parent }) as Component<TParent>;
				if (createdComponent != null && Parent.TryAddComponent(createdComponent))
				{
					dependency.Set(this, createdComponent);
				}
				else
				{
					throw new Exception($"Unable to setup the dependencies for the component {GetType()} as it dependency on a component that was not found and could not be added ({dependency.dependencyType}). We could not create the new component, causing a panic.");
				}
			}
		}
		// Return if we setup the dependencies in a valid manner or not.
		return valid;
	}
#else
		/// <summary>
		/// Sets up the dependencies for this component, must be called before
		/// the component is ready to use.
		/// Dependencies allow for direct function calls between 2 different components,
		/// for cases where you may need exactly 1 call, rather than the 0 to many calls
		/// that signals offer.
		/// </summary>
		/// <returns></returns>
		internal bool SetupDependencies()
		{
			// Try to access the dependency cache, or build it
			if (!DependencyCache.TryGetValue(GetType(), out IEnumerable<(DependencyAttribute attribute, ISharedDependency dependency)> dependencies))
			{
				dependencies = GetType()
					.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
					.Where(x => x.GetCustomAttribute<DependencyAttribute>() != null && typeof(Component<TParent>).IsAssignableFrom(x.FieldType))
					.Select(x => (x.GetCustomAttribute<DependencyAttribute>(), (ISharedDependency)new FieldDependency(x)))
					.Union(
						GetType()
							.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
							.Where(x => x.GetCustomAttribute<DependencyAttribute>() != null && typeof(Component<TParent>).IsAssignableFrom(x.PropertyType))
							.Select(x => (x.GetCustomAttribute<DependencyAttribute>(), (ISharedDependency)new PropertyDependency(x)))
					);
				DependencyCache.TryAdd(GetType(), dependencies);
			}
			bool valid = true;
			// Now that we have our dependencies, set them up
			foreach ((DependencyAttribute attribute, ISharedDependency dependency) dep in dependencies)
			{
				ISharedDependency dependency = dep.dependency;
				// Try to locate the component, or error
				if (Parent.TryGetComponent(dependency.dependencyType, out Component<TParent> located))
				{
					dependency.Set(this, located);
				}
				else if (!(dep.attribute.Optional && dep.dependency.Nullable))
				{
					// Failed to fetch the required dependency, throw a warning and add it
					valid = false;
					// We might fail to add the component, in which case I have no idea what to do
					if (Activator.CreateInstance(dependency.dependencyType, new object[] { Parent }) is Component<TParent> createdComponent && Parent.TryAddComponent(createdComponent))
					{
						dependency.Set(this, createdComponent);
					}
					else
					{
						throw new Exception($"Unable to setup the dependencies for the component {GetType()} as it dependency on a component that was not found and could not be added ({dependency.dependencyType}). We could not create the new component, causing a panic.");
					}
				}
			}
			// Return if we setup the dependencies in a valid manner or not.
			return valid;
		}
#endif

		private interface ISharedDependency
		{
			Type dependencyType { get; }
			bool Nullable { get; }
			void Set(Component<TParent> source, Component<TParent> value);
		}

		private class FieldDependency : ISharedDependency
		{
			public FieldInfo fieldInfo;

			public FieldDependency(FieldInfo fieldInfo)
			{
				this.fieldInfo = fieldInfo;
				dependencyType = fieldInfo.FieldType;
				if (typeof(Nullable<>).IsAssignableFrom(dependencyType))
				{
					Nullable = true;
					dependencyType = dependencyType.GenericTypeArguments[0];
				}
			}

			public Type dependencyType { get; }

			public bool Nullable { get; }

			public void Set(Component<TParent> source, Component<TParent> value)
			{
				fieldInfo.SetValue(source, value);
			}
		}

		private class PropertyDependency : ISharedDependency
		{
			public PropertyInfo propertyInfo;

			public PropertyDependency(PropertyInfo propertyInfo)
			{
				this.propertyInfo = propertyInfo;
				dependencyType = propertyInfo.PropertyType;
				if (typeof(Nullable<>).IsAssignableFrom(dependencyType))
				{
					Nullable = true;
					dependencyType = dependencyType.GenericTypeArguments[0];
				}
			}

			public Type dependencyType { get; }

			public bool Nullable { get; }

			public void Set(Component<TParent> source, Component<TParent> value)
			{
				propertyInfo.SetValue(source, value);
			}
		}

	}
}