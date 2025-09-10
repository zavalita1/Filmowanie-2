namespace Filmowanie.Database.Interfaces.ReadOnlyEntities;

public interface IReadOnlyMovieEntity : IReadOnlyEntity
{
    string Name { get;  }
    string OriginalTitle { get;  }
    string Description { get;  }
    string PosterUrl { get;  }
    string BigPosterUrl { get;  }
    string FilmwebUrl { get;  }
    string[] Actors { get;  }
    string[] Writers { get;  }
    string[] Directors { get;  }
    string[] Genres { get;  }
    int CreationYear { get;  }
    int DurationInMinutes { get;  }
    string Type { get; }

    /// <summary>
    /// Movie has been nominated and then rejected for some reason. Most likely, cause someone already watched it, or it's said to contain sensitive triggers.
    /// </summary>
    bool? IsRejected { get; }

    string id { get;  }
}