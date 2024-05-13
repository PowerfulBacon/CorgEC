using Assets.Code.Networking.Communication.ApplicationLayer;
using Assets.Code.Networking.Communication.NetworkLayer;
using Assets.Code.Networking.Communication.Packets;
using Assets.Code.Networking.Serialisation;
using ECCore.Components;
using ECCore.Instances;
using ECSCore.Signals;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// The base of logical entities in the game world,
/// seperated from their representation in Unity.
/// </summary>
public sealed partial class Entity : SignalHolder, IEnumerable<Entity>, INetworkedSerialised
{

	public bool Initialized { get; private set; } = false;
	public bool Destroyed { get; private set; } = false;

    /// <summary>
    /// Needs to be created via helpers
    /// </summary>
    protected Entity(Instance instance) : base(instance)
	{ }

	internal void JoinInstance(Instance instance)
	{
		Instance = instance;
	}

	/// <summary>
	/// Create a new entity, adding any components that we need to add to it in the
	/// passed initialisation function.
	/// </summary>
	/// <param name="entityCreation"></param>
	/// <returns></returns>
	public static Entity Create(Instance instance, Action<Entity> entityCreation)
	{
		Entity entity = new Entity(instance);
		entityCreation?.Invoke(entity);
		entity.Initialise();
		return entity;
	}

	public void Initialise()
	{
		ValidateComponents();
		InitialiseComponents();
		Initialized = true;
		// Tell other instances that an entity was created
		if (Instance.IsHostInstance())
		{
			new EntityCreation(this).SendToOthers(Instance.NetworkManager);
        }
	}

	public void Destroy()
	{
		// Remove from old parent's contents, but keep location
		if (Location != null)
			Location.RemoveFromContents(this);
		// Remove all our components
		RemoveComponents();
		// Now, remove our location.
		Location = null;
		// Destroy the entity
		Destroyed = true;
		// Tell others that we are destroyed
		if (Instance.IsHostInstance())
		{
			// TODO: Send this to only the clients that are aware of this entity.
			new EntityDestroy(this).SendToOthers(Instance.NetworkManager);
        }
	}

	#region Ownership

	public bool IsLocalOwner()
	{
		return true;
	}

    #endregion

    #region Components

    private List<IComponent> components = new List<IComponent>();

	/// <summary>
	/// The components, implemented as a list as we shouldn't need to get components
	/// a lot as the dependency system handles caching them for direct access.
	/// Use the [Dependency] attribute if you need to frequently interface with another specific
	/// component.
	/// </summary>
	public IReadOnlyList<IComponent> Components => components;

	public bool HasComponent<T>()
        where T : Component<T>
    {
        return components
            .Where(x => x.GetType() == typeof(T))
            .Any();
    }

	public T GetComponent<T>()
		where T : Component<T>
	{
		if (Destroyed)
			throw new NullReferenceException("Attempting to access a destroyed entity.");
		return (T)components
			.Where(x => x.GetType() == typeof(T))
			.Single();
	}

	public T GetOrAddComponent<T>(Func<T> componentCreator)
		where T : Component<T>
	{
		if (Destroyed)
			throw new NullReferenceException("Attempting to access a destroyed entity.");
		var result = components
			.Where(x => x.GetType() == typeof(T))
			.SingleOrDefault() as T;
		if (result != default)
			return result;
		var createdComponent = componentCreator();
		if (TryAddComponent(createdComponent))
			return createdComponent;
		throw new Exception("Could not add the created component to the entity.");
	}

#if NET6_0_OR_GREATER
	public bool TryGetComponent<T>(out T? component)
#else
    public bool TryGetComponent<T>(out T component)
#endif
        where T : Component<T>
	{
		if (Destroyed)
			throw new NullReferenceException("Attempting to access a destroyed entity.");
		component = components
			.Where(x => x.GetType() == typeof(T))
			.SingleOrDefault() as T;
		return component != default;
	}

	public IComponent GetComponent(Type componentType)
	{
		if (Destroyed)
			throw new NullReferenceException("Attempting to access a destroyed entity.");
		return components
			.Where(x => x.GetType() == componentType)
			.Single();
	}

#if NET6_0_OR_GREATER
	public bool TryGetComponent(Type componentType, out IComponent? component, bool allowSubtypes = true)
	{
		if (Destroyed)
			throw new NullReferenceException("Attempting to access a destroyed entity.");
		component = components
			.Where(x => allowSubtypes ? componentType.IsAssignableFrom(x.GetType()) : x.GetType() == componentType)
			.SingleOrDefault();
		return component != default;
	}
#else
	public bool TryGetComponent(Type componentType, out IComponent component, bool allowSubtypes = true)
	{
		if (Destroyed)
			throw new NullReferenceException("Attempting to access a destroyed entity.");
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
    public bool TryAddComponent(IComponent component)
	{
		if (Destroyed)
			throw new NullReferenceException("Attempting to access a destroyed entity.");
		if (Initialized)
		{
			try
			{
				component.Parent = this;
				component._SetupDependencies();
				component._Initialise();
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
			component.Parent = this;
			components.Add(component);
			return true;
		}
	}

	public bool RemoveComponent(IComponent component)
    {
        if (Destroyed)
            throw new NullReferenceException("Attempting to access a destroyed entity.");
		components.Remove(component);
		component._Remove();
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
			.All(component => component._SetupDependencies());
	}

	internal void InitialiseComponents()
	{
		foreach (IComponent component in Components)
			component._Initialise();
	}

	internal void RemoveComponents()
	{
		var componentList = Components.ToList();
		components.Clear();
		foreach (IComponent component in componentList)
			component._Remove();
	}

#endregion

	#region Entity Contents

#if NET6_0_OR_GREATER
	/// <summary>
	/// A nullable list of entities that are stored
	/// inside of this entity.
	/// </summary>
	private List<Entity>? Contents = null;

	public Entity? Location { get; private set; }
#else
	/// <summary>
	/// A nullable list of entities that are stored
	/// inside of this entity.
	/// </summary>
	private List<Entity> Contents = null;

	public Entity Location { get; private set; }
#endif

    public void Move(Entity newEntity)
	{
		if (Equals(newEntity))
			throw new Exception("Cannot move entity inside of itself.");
#if RECURSIVE_CONTENTS_DEBUGGING
		Entity current = newEntity;
		while (current != null && current.Location != null)
		{
			current = current.Location;
			if (current == this)
				throw new Exception("Attempting to move an entity inside of itself, creating a recursive entity stack!");
		}
#endif
		// Remove from old parent's contents
		if (Location != null)
			Location.RemoveFromContents(this);
		Location = newEntity;
		// Insert into new contents
		if (Location != null)
			Location.PutInContents(this);
	}

	internal void PutInContents(Entity entity)
	{
		(Contents ?? (Contents = new List<Entity>())).Add(entity);
	}

	internal void RemoveFromContents(Entity entity)
	{
		(Contents ?? (Contents = new List<Entity>())).Remove(entity);
	}

	public IEnumerator<Entity> GetEnumerator()
	{
		if (Destroyed)
			throw new NullReferenceException("Attempting to access a destroyed entity.");
		if (Contents == null)
			yield break;
		foreach (Entity entity in Contents)
			yield return entity;
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		if (Destroyed)
			throw new NullReferenceException("Attempting to access a destroyed entity.");
		if (Contents == null)
			yield break;
		foreach (Entity entity in Contents)
			yield return entity;
	}

#endregion

	#region Operators

	public static bool operator true(Entity entity)
	{
		return entity != null && !entity.Destroyed;
	}

	public static bool operator false(Entity entity)
	{
		return entity == null || entity.Destroyed;
	}

	public static bool operator !(Entity entity)
	{
		return entity == null || entity.Destroyed;
	}

	public static bool operator &(Entity entity, bool other)
	{
		if (!entity)
			return false;
		return other;
	}

#if NET6_0_OR_GREATER
	public static bool operator ==(Entity? entity, object? other)
	{
		if (entity is null)
		{
			if (other is Entity otherEntity)
				return otherEntity.Destroyed;
			return other is null;
		}
		if (other is not Entity)
		{
			if (other is null && entity.Destroyed)
				return true;
			return false;
		}
		if (other is null)
			return entity.Destroyed;
		if (entity.Destroyed && ((Entity)other).Destroyed)
			return true;
		return entity.Equals(other);
	}

	public static bool operator !=(Entity? entity, object? other)
	{
		return !(entity == other);
	}

	public override bool Equals(object? obj)
	{
		return base.Equals(obj);
	}
#else
    public static bool operator ==(Entity entity, object other)
    {
        if (entity is null)
        {
            if (other is Entity otherEntity)
                return otherEntity.Destroyed;
            return other is null;
        }
        if (!(other is Entity))
        {
            if (other is null && entity.Destroyed)
                return true;
            return false;
        }
        if (other is null)
            return entity.Destroyed;
        if (entity.Destroyed && ((Entity)other).Destroyed)
            return true;
        return entity.Equals(other);
    }

    public static bool operator !=(Entity entity, object other)
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

    public uint NetworkID { get; set; }

    public void Serialise(INetworkInterface target, BinaryWriter writer)
    {
		writer.Write(Initialized);
        writer.Write(Destroyed);
		SerialisationHelper.Serialise(target, typeof(Entity), Location, writer);
		writer.Write(Contents?.Count ?? 0);
		if (Contents != null)
		{
			foreach (var entity in Contents)
			{
				SerialisationHelper.Serialise(target, typeof(Entity), entity, writer);
			}
		}
		writer.Write(Components.Count);
		foreach (var component in Components)
        {
            SerialisationHelper.Serialise(target, typeof(IComponent), component, writer);
        }
    }

    public void Deserialise(INetworkInterface sender, BinaryReader reader)
    {
		Initialized = reader.ReadBoolean();
		Destroyed = reader.ReadBoolean();
		Location = (Entity)SerialisationHelper.Deserialise(sender, Instance.NetworkManager, typeof(Entity), reader);
		var count = reader.ReadInt32();
		if (count == 0)
		{
			Contents = null;
		}
		else
		{
			Contents = new List<Entity>();
            Contents.Clear();
			for (int i = 0; i < count; i++)
			{
				Contents.Add((Entity)SerialisationHelper.Deserialise(sender, Instance.NetworkManager, typeof(Entity), reader));
			}
		}
		components.Clear();
        for (int i = 0; i < reader.ReadInt32(); i++)
        {
            components.Add((IComponent)SerialisationHelper.Deserialise(sender, Instance.NetworkManager, typeof(IComponent), reader));
        }
    }

}

internal class EntityCreation : Packet<EntityCreation>
{

	public Entity createdEntity;

    public EntityCreation(Entity createdEntity)
    {
        this.createdEntity = createdEntity;
    }

    protected override void Recieve(NetworkManager localNetworkManager, INetworkInterface sender, double sendTime)
    {
		Instance instance = Instance.InstanceFromNetwork(localNetworkManager);
		createdEntity.JoinInstance(instance);
		// We already have components from the point of initialisation
		createdEntity.Initialise();
    }

}

internal class EntityDestroy : Packet<EntityDestroy>
{

    public Entity destroyedEntity;

    public EntityDestroy(Entity destroyedEntity)
    {
        this.destroyedEntity = destroyedEntity;
    }

    protected override void Recieve(NetworkManager localNetworkManager, INetworkInterface sender, double sendTime)
    {
        destroyedEntity.Destroy();
    }

}
