using UnityEngine;

public interface ITrigger
{
    void Trigger(Player player);
    Transform Transform { get; }
}