using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISubCommand
{

}
public abstract class SubCommand : MonoBehaviour
{
    public abstract List<CommandData> LoadDataList();

    public abstract string GetFormattedList();




}
