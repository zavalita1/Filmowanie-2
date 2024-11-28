namespace Filmowanie.Database.Interfaces.ReadOnlyEntities;

public interface IReadOnlyMovieEntity : IReadOnlyEntity
{
    string Name { get;  }
    string OriginalTitle { get;  }
    string Description { get;  }
    string PosterUrl { get;  }
    string FilmwebUrl { get;  }
    string[] Actors { get;  }
    string[] Writers { get;  }
    string[] Directors { get;  }
    string[] Genres { get;  }
    int CreationYear { get;  }
    int DurationInMinutes { get;  }
    int TenantId { get;  }

    string Type { get; }

    string id { get;  }
}