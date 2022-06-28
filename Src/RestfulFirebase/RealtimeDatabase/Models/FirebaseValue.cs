using RestfulFirebase.RealtimeDatabase.Realtime;
using RestfulFirebase.Exceptions;
using RestfulFirebase.Utilities;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RestfulFirebase.Local;
using SerializerHelpers;
using SerializerHelpers.Exceptions;
using RestfulFirebase.Abstraction;
using CommunityToolkit.Mvvm.ComponentModel;
using RestfulFirebase.RealtimeDatabase.Attributes;

namespace RestfulFirebase.RealtimeDatabase.Models;

public abstract class FirebaseValue : RealtimeModel, INullableObject
{
    #region Properties



    #endregion

    #region Methods

    /// <summary>
    /// Sets the object of the property.
    /// </summary>
    /// <param name="value">
    /// The value of the property.
    /// </param>
    /// <returns>
    /// <c>true</c> whether the property has changed; otherwise <c>false</c>.
    /// </returns>
    public abstract bool SetObject(object? value);

    /// <summary>
    /// Gets the object of the property.
    /// </summary>
    /// <returns>
    /// The value of the property.
    /// </returns>
    public abstract object? GetObject();

    #endregion

    #region INullableObject Members

    /// <inheritdoc/>
    public abstract bool IsNull();

    /// <inheritdoc/>
    public abstract bool SetNull();

    #endregion
}

[ObservableObject]
public partial class FirebaseValue<T> : FirebaseValue
{
    #region Properties

    /// <summary>
    /// Gets the value of the property.
    /// </summary>
    public T Value
    {
        get => GetValue();
        //set => SetValue(value);
    }

    private object? value;
    private Type? type;
    private bool isValueCached;

    #endregion

    #region Initializers

    public FirebaseValue()
    {

    }

    #endregion

    #region Methods



    #endregion

    #region FirebaseValue Members

    public override bool SetObject(object? value)
    {
        throw new NotImplementedException();
    }

    public override object? GetObject()
    {
        throw new NotImplementedException();
    }

    //public bool SetValue(T value)
    //{
    //    return RWLock.LockWrite(() =>
    //    {
    //        if (SetProperty(ref this.value, value, nameof(Value)))
    //        {
    //            return true;
    //        }
    //        return false;
    //    });


    //    if (IsDisposed)
    //    {
    //        return false;
    //    }

    //    if (value is RealtimeModel model)
    //    {
    //        if (SetProperty(ref this.value, value, nameof(Value)))
    //        {
    //            if (RealtimeInstance != null)
    //            {
    //                if (model.RealtimeInstance != RealtimeInstance)
    //                {
    //                    model.AttachRealtime(RealtimeInstance, true);
    //                }
    //            }

    //            return true;
    //        }
    //    }
    //    else
    //    {
    //        string? blob = type == null ? null : Serializer.Serialize(obj, type);

    //        if (base.InternalSetObject(null, blob))
    //        {
    //            if (currentType != null)
    //            {
    //                hasObjChanges = !(currentValue?.Equals(obj) ?? obj == null);
    //            }
    //            else
    //            {
    //                hasObjChanges = true;
    //            }

    //            currentValue = obj;
    //            currentType = type;
    //            isValueCached = true;

    //            RealtimeInstance? instance = RealtimeInstance;
    //            if (instance != null && !instance.IsDisposed)
    //            {
    //                if (hasPostAttachedRealtime)
    //                {
    //                    Task.Run((Func<Task>)(async () =>
    //                    {
    //                        while (true)
    //                        {
    //                            if (!hasPostAttachedRealtime)
    //                            {
    //                                instance = RealtimeInstance;
    //                                if (instance?.IsDisposed ?? true)
    //                                {
    //                                    return;
    //                                }
    //                                break;
    //                            }
    //                            await Task.Delay((TimeSpan)instance.App.Config.DatabaseRetryDelay);
    //                        }

    //                        instance?.SetValue(blob);
    //                    }));
    //                }
    //                else
    //                {
    //                    instance.SetValue(blob);
    //                }
    //            }
    //        }
    //    }

    //    return hasObjChanges;
    //}

    public T GetValue()
    {
        throw new NotImplementedException();
    }

    #endregion

    #region RealtimeModel Members

    internal override void RealtimeInstanceAttaching()
    {
        throw new NotImplementedException();
    }

    internal override void RealtimeInstanceDetaching()
    {
        throw new NotImplementedException();
    }

    internal override void RealtimeInstanceDataChanges(DataChangesEventArgs e)
    {
        throw new NotImplementedException();
    }

    #endregion

    #region INullableObject Members

    public override bool IsNull()
    {
        throw new NotImplementedException();
    }

    public override bool SetNull()
    {
        throw new NotImplementedException();
    }

    #endregion
}
