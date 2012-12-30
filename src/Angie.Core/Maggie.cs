﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Angela.Core.Fillers;

namespace Angela.Core
{
    internal class Maggie
    {
        private IDictionary<Type, IList<IPropertyFiller>> _specificPropertyFillersByObjectType;
        private IDictionary<Type, IPropertyFiller> _genericPropertyFillersByPropertyType;

        public Maggie()
        {
            _specificPropertyFillersByObjectType = new Dictionary<Type, IList<IPropertyFiller>>();
            IList<IPropertyFiller> objectFillers = new List<IPropertyFiller>();
            objectFillers.Add(new FirstNameFiller());
            objectFillers.Add(new AgeFiller());
            objectFillers.Add(new EmailFiller());
            _specificPropertyFillersByObjectType.Add(typeof(object), objectFillers);

         

            _genericPropertyFillersByPropertyType = new Dictionary<Type, IPropertyFiller>();
            _genericPropertyFillersByPropertyType.Add(typeof(string), new GenericStringFiller());
            _genericPropertyFillersByPropertyType.Add(typeof(int), new GenericIntFiller());
        }

        public void RegisterFiller(IPropertyFiller filler)
        {
            if (_specificPropertyFillersByObjectType.ContainsKey(filler.ObjectType))
            {
                _specificPropertyFillersByObjectType[filler.ObjectType].Add(filler);
            }
            else
            {
                List<IPropertyFiller> fillers = new List<IPropertyFiller>();
                fillers.Add(filler);
                _specificPropertyFillersByObjectType.Add(filler.ObjectType, fillers);
            }
        }

        public IPropertyFiller GetFiller(PropertyInfo propertyInfo)
        {
            IPropertyFiller result = null;
            Type objectType = propertyInfo.DeclaringType;
            while (objectType != null)
            {
                if (_specificPropertyFillersByObjectType.ContainsKey(objectType))
                {
                    foreach (IPropertyFiller propertyFiller in _specificPropertyFillersByObjectType[objectType  ])
                    {
                        if (propertyFiller.PropertyType == propertyInfo.PropertyType &&
                            propertyFiller.PropertyNames.Any(s =>  s.ToLowerInvariant() == propertyInfo.Name.ToLower()))
                        {
                            result = propertyFiller;
                            break;
                        }
                    }
                }

                objectType = objectType.BaseType;
            }

            if (result == null)
            {
                if (_genericPropertyFillersByPropertyType.ContainsKey(propertyInfo.PropertyType))
                {
                    result = _genericPropertyFillersByPropertyType[propertyInfo.PropertyType];
                }
                else
                {
                    result = new CustomFiller("*", typeof (object), propertyInfo.PropertyType, () => null);
                    _genericPropertyFillersByPropertyType.Add(propertyInfo.PropertyType, result);
                }
            }

            return result;
        }

        public void SetMinInt(int min)
        {
            GenericIntFiller intFiller = (GenericIntFiller) _genericPropertyFillersByPropertyType[typeof(int)];
            intFiller.Min = min;
        }

        public void SetMaxInt(int max)
        {
            GenericIntFiller intFiller = (GenericIntFiller)_genericPropertyFillersByPropertyType[typeof(int)];
            intFiller.Min = max;
        }
    }
}