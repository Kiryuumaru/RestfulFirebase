using RestfulFirebase.Http;
using System.Collections.Generic;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace RestfulFirebase;

/// <summary>
/// Provides configuration for all operation.
/// </summary>
public partial class FirebaseConfig : INotifyPropertyChanged, INotifyPropertyChanging
{
    /// <summary>
    /// Gets or sets the firebase API key.
    /// </summary>
    public string ApiKey { get; }

    /// <summary>
    /// Gets or sets the firebase API key.
    /// </summary>
    public string ProjectId { get; }

    /// <summary>
    /// Gets or sets the <see cref="IHttpClientFactory"/>.
    /// </summary>
    public IHttpClientFactory? HttpClientFactory
    {
        get => httpClientFactory;
        set
        {
            if (!EqualityComparer<IHttpClientFactory?>.Default.Equals(httpClientFactory, value))
            {
                OnPropertyChanging();
                httpClientFactory = value;
                OnPropertyChanged();
            }
        }
    }
    IHttpClientFactory? httpClientFactory;

    /// <summary>
    /// Gets or sets the <see cref="IHttpStreamFactory"/>.
    /// </summary>
    public IHttpStreamFactory? HttpStreamFactory
    {
        get => httpStreamFactory;
        set
        {
            if (!EqualityComparer<IHttpStreamFactory?>.Default.Equals(httpStreamFactory, value))
            {
                OnPropertyChanging();
                httpStreamFactory = value;
                OnPropertyChanged();
            }
        }
    }
    IHttpStreamFactory? httpStreamFactory;

    /// <summary>
    /// Gets or sets the <see cref="TimeSpan"/> timeout used for the firebase realtime database unresponsive streamers.
    /// </summary>
    public TimeSpan DatabaseColdStreamTimeout
    {
        get => databaseColdStreamTimeout;
        set
        {
            if (!EqualityComparer<TimeSpan>.Default.Equals(databaseColdStreamTimeout, value))
            {
                OnPropertyChanging();
                databaseColdStreamTimeout = value;
                OnPropertyChanged();
            }
        }
    }
    TimeSpan databaseColdStreamTimeout = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Gets or sets the <see cref="TimeSpan"/> retry delay for the firebase realtime database failed requests.
    /// </summary>
    public TimeSpan DatabaseRetryDelay
    {
        get => databaseRetryDelay;
        set
        {
            if (!EqualityComparer<TimeSpan>.Default.Equals(databaseRetryDelay, value))
            {
                OnPropertyChanging();
                databaseRetryDelay = value;
                OnPropertyChanged();
            }
        }
    }
    TimeSpan databaseRetryDelay = TimeSpan.FromSeconds(2);

    /// <summary>
    /// Gets or sets the default <see cref="System.Text.Json.JsonSerializerOptions"/>.
    /// </summary>
    public JsonSerializerOptions? JsonSerializerOptions
    {
        get => jsonSerializerOptions;
        set
        {
            if (!EqualityComparer<JsonSerializerOptions?>.Default.Equals(jsonSerializerOptions, value))
            {
                OnPropertyChanging();
                jsonSerializerOptions = value;
                OnPropertyChanged();
            }
        }
    }
    JsonSerializerOptions? jsonSerializerOptions;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc/>
    public event PropertyChangingEventHandler? PropertyChanging;

    /// <summary>
    /// Creates new instance of <see cref="FirebaseConfig"/> with the default configurations.
    /// </summary>
    /// <param name="apiKey">
    /// The API key of the app.
    /// </param>
    /// <param name="projectId">
    /// The project ID of the app.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="projectId"/> or
    /// <paramref name="projectId"/> is a null reference.
    /// </exception>
    public FirebaseConfig(string projectId, string apiKey)
    {
        ArgumentNullException.ThrowIfNull(projectId);
        ArgumentNullException.ThrowIfNull(apiKey);

        ApiKey = apiKey;
        ProjectId = projectId;
    }

    /// <summary>
    /// Raises the <see cref = "PropertyChanged"/> event.
    /// </summary>
    /// <param name = "propertyName">
    /// (optional) The name of the property that changed.
    /// </param>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Raises the <see cref = "PropertyChanging"/> event.
    /// </summary>
    /// <param name = "propertyName">
    /// (optional) The name of the property that changed.
    /// </param>
    protected void OnPropertyChanging([CallerMemberName] string? propertyName = null)
    {
        OnPropertyChanging(new PropertyChangingEventArgs(propertyName));
    }

    /// <summary>
    /// Raises <see cref = "PropertyChanged"/>.
    /// </summary>
    /// <param name = "e">
    /// The input <see cref = "PropertyChangedEventArgs"/> instance.
    /// </param>
    protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        PropertyChanged?.Invoke(this, e);
    }

    /// <summary>
    /// Raises <see cref = "PropertyChanging"/>.
    /// </summary>
    /// <param name = "e">
    /// The input <see cref = "PropertyChangingEventArgs"/> instance.
    /// </param>
    protected virtual void OnPropertyChanging(PropertyChangingEventArgs e)
    {
        PropertyChanging?.Invoke(this, e);
    }
}
