using Momento.Sdk.Messages.Vector;

namespace Momento.Sdk.Responses.Vector;

using System.Collections.Generic;

/// <summary>
/// A hit from a vector search. Contains the ID of the vector, the search score,
/// and any requested metadata.
/// </summary>
public class SearchHit
{
    /// <summary>
    /// The ID of the hit.
    /// </summary>
    public string Id { get; }
    
    /// <summary>
    /// The similarity to the query vector.
    /// </summary>
    public double Score { get; }
    
    /// <summary>
    /// Requested metadata associated with the hit.
    /// </summary>
    public Dictionary<string, MetadataValue> Metadata { get; }
    
    /// <summary>
    /// Constructs a SearchHit with no metadata.
    /// </summary>
    /// <param name="id">The ID of the hit.</param>
    /// <param name="score">The similarity to the query vector.</param>
    public SearchHit(string id, double score)
    {
        Id = id;
        Score = score;
        Metadata = new Dictionary<string, MetadataValue>();
    }
    
    /// <summary>
    /// Constructs a SearchHit.
    /// </summary>
    /// <param name="id">The ID of the hit.</param>
    /// <param name="score">The similarity to the query vector.</param>
    /// <param name="metadata">Requested metadata associated with the hit</param>
    public SearchHit(string id, double score, Dictionary<string, MetadataValue> metadata)
    {
        Id = id;
        Score = score;
        Metadata = metadata;
    }

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
        if (ReferenceEquals(this, obj)) return true;
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (obj is null || GetType() != obj.GetType()) return false;

        var other = (SearchHit)obj;

        // ReSharper disable once CompareOfFloatsByEqualityOperator
        if (Id != other.Id || Score != other.Score) return false;
        
        // Compare Metadata dictionaries
        if (Metadata.Count != other.Metadata.Count) return false;

        foreach (var pair in Metadata)
        {
            if (!other.Metadata.TryGetValue(pair.Key, out var value)) return false;
            if (!value.Equals(pair.Value)) return false;
        }

        return true;
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        unchecked // Overflow is fine, just wrap
        {
            var hash = 17;

            hash = hash * 23 + Id.GetHashCode();
            hash = hash * 23 + Score.GetHashCode();

            foreach (var pair in Metadata)
            {
                hash = hash * 23 + pair.Key.GetHashCode();
                hash = hash * 23 + (pair.Value?.GetHashCode() ?? 0);
            }

            return hash;
        }
    }
}
