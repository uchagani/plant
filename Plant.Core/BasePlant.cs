﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Plant.Core
{
  public class BasePlant
  {
    private readonly IDictionary<Type, object> blueprints = new Dictionary<Type, object>();

    public BasePlant()
    {
      
    }

    public virtual T Create<T>() where T : new()
    {
      var instance = new T();
      if(blueprints.ContainsKey(typeof(T)))
        SetProperties(blueprints[typeof(T)], instance);
      return instance;
    }

    public virtual T Create<T>(object userSpecifiedProperties) where T : new()
    {
      var instance = Create<T>();
      SetProperties(userSpecifiedProperties, instance);
      return instance;
    }

    private static void SetProperties<T>(object propertyValues, T instance)
    {
      var properties = propertyValues.GetType().GetProperties().ToList();
      properties.ForEach(property =>
                                    {
                                      var instanceProperty = instance.GetType().GetProperties().FirstOrDefault(prop => prop.Name == property.Name);
                                      if(instanceProperty == null) throw new PropertyNotFoundException();
                                      var value = property.GetValue(propertyValues, null);
                                      instanceProperty.SetValue(instance, value, null);
                                    });
    }

    public virtual void Define<T>(object defaults)
    {
      blueprints.Add(typeof(T), defaults);
    }

    public BasePlant WithBlueprintsFromAssemblyOf<T>()
    {
      var assembly = typeof (T).Assembly;
      var blueprintTypes = assembly.GetTypes().Where(t => typeof (Blueprint).IsAssignableFrom(t));
      blueprintTypes.ToList().ForEach(blueprintType =>
                                    {
                                      var blueprint = (Blueprint)Activator.CreateInstance(blueprintType);
                                      blueprint.SetupPlant(this);
                                    });
      return this;

    }
  }
}