using System;
using System.Collections.Generic;
using System.Linq;

namespace GameWatch.Core;

public class DbStateMng
{
  private readonly IFileSys _fileSys;
  private readonly FileInfo _state1;
  private readonly FileInfo _state2;
  private readonly FileInfo _dbOriginal;
  private readonly FileInfo _dbBackup;
  private readonly Dictionary<StateValue, string> _stateValueStrings;

  // ReSharper disable once ConvertToPrimaryConstructor
  public DbStateMng(IFileSys fileSys, FileInfo state1, FileInfo state2, FileInfo dbOriginal, FileInfo dbBackup)
  {
    _fileSys = fileSys;
    _state1 = state1;
    _state2 = state2;
    _dbOriginal = dbOriginal;
    _dbBackup = dbBackup;

    _stateValueStrings = new()
                         {
                           { StateValue.Idling, ((int)StateValue.Idling).ToString() },
                           { StateValue.MakingDbBackup, ((int)StateValue.MakingDbBackup).ToString() },
                           { StateValue.WasUpdatingOriginalDb, ((int)StateValue.WasUpdatingOriginalDb).ToString() }
                         };
  }

  public enum StateValue
  {
    Idling,
    MakingDbBackup,
    WasUpdatingOriginalDb,
  }

  public string StateValueIndexAsString(StateValue value) => _stateValueStrings[value];

  public void HealthCheck()
  {
    var state1Exists = _fileSys.CheckExists(_state1);
    var state2Exists = _fileSys.CheckExists(_state2);

    if (!state1Exists && !state2Exists)
    {
      _fileSys.WriteText(_state1, StateValueIndexAsString(StateValue.Idling));
      _fileSys.WriteText(_state2, StateValueIndexAsString(StateValue.Idling));
    }
    else if (state1Exists && state2Exists)
    {
      var state1Contents = _fileSys.ReadText(_state1);
      var state2Contents = _fileSys.ReadText(_state2);

      if (state1Contents == state2Contents)
      {
        var stateContents = state1Contents;
        var stateContentsValid = ValidateStateFileContents(stateContents);

        if (stateContentsValid)
        {
          if (stateContents == StateValueIndexAsString(StateValue.Idling))
          {
            _fileSys.Delete(_dbBackup);
          }
          else if (stateContents == StateValueIndexAsString(StateValue.MakingDbBackup))
          {
            _fileSys.Delete(_dbBackup);
            _fileSys.WriteText(_state1, StateValueIndexAsString(StateValue.Idling));
            _fileSys.WriteText(_state2, StateValueIndexAsString(StateValue.Idling));
          }
          else if (stateContents == StateValueIndexAsString(StateValue.WasUpdatingOriginalDb))
          {
            throw new NotImplementedException("Unexpected branch reached");
          }
          else
          {
            throw new NotImplementedException("Unexpected branch reached");
          }
        }
        else if (!stateContentsValid)
        {
          throw new NotImplementedException("Unexpected branch reached");
        }
        else
        {
          throw new NotImplementedException("Unexpected branch reached");
        }
      }
      else if (state1Contents != state2Contents)
      {
        var state1ContentsValid = ValidateStateFileContents(state1Contents);
        var state2ContentsValid = ValidateStateFileContents(state2Contents);

        if (state1ContentsValid && state2ContentsValid)
        {
          throw new NotImplementedException("Unexpected branch reached");
        }
        else if (!state1ContentsValid && !state2ContentsValid)
        {
          throw new NotImplementedException("Unexpected branch reached");
        }
        else if (!state1ContentsValid && state2ContentsValid)
        {
          if (state2Contents == StateValueIndexAsString(StateValue.Idling))
          {
            _fileSys.WriteText(_state1, StateValueIndexAsString(StateValue.Idling));
          }
          else if (state2Contents == StateValueIndexAsString(StateValue.MakingDbBackup))
          {
            throw new NotImplementedException("Unexpected branch reached");
          }
          else if (state2Contents == StateValueIndexAsString(StateValue.WasUpdatingOriginalDb))
          {
            throw new NotImplementedException("Unexpected branch reached");
          }
          else
          {
            throw new NotImplementedException("Unexpected branch reached");
          }
        }
        else if (state1ContentsValid && !state2ContentsValid)
        {
          throw new NotImplementedException("Unexpected branch reached");
        }
        else
        {
          throw new NotImplementedException("Unexpected branch reached");
        }
      }
      else
      {
        throw new NotImplementedException("Unexpected branch reached");
      }
    }
    else if (state1Exists && !state2Exists)
    {
      throw new NotImplementedException("Unexpected branch reached");
    }
    else if (!state1Exists && state2Exists)
    {
      throw new NotImplementedException("Unexpected branch reached");
    }
    else
    {
      throw new NotImplementedException("Unexpected branch reached");
    }
  }

  private bool ValidateStateFileContents(string stateFileContents) => _stateValueStrings.Values.Any(v => v == stateFileContents);
}

// var state1Contents = _fileSys.ReadText(_state1);
// var state2Contents = _fileSys.ReadText(_state2);
//
// if (state1Contents == state2Contents)
// {
//   if (state1Contents == StateValueIndexAsString(StateValue.Idling))
//   {
//     _fileSys.Delete(_dbBackup);
//     return;
//   }
//
//   if (state1Contents == StateValueIndexAsString(StateValue.MakingDbBackup))
//   {
//     _fileSys.Delete(_dbBackup);
//     _fileSys.WriteText(_state1, StateValueIndexAsString(StateValue.Idling));
//     _fileSys.WriteText(_state2, StateValueIndexAsString(StateValue.Idling));
//     return;
//   }
//
//   if (state1Contents == StateValueIndexAsString(StateValue.WasUpdatingOriginalDb))
//   {
//     _fileSys.Copy(src: _dbBackup, dest: _dbOriginal, overwrite: true);
//     _fileSys.WriteText(_state1, StateValueIndexAsString(StateValue.Idling));
//     _fileSys.WriteText(_state2, StateValueIndexAsString(StateValue.Idling));
//     return;
//   }
//
//   throw new NotImplementedException("Unexpected branch reached");
// }
//
// var state1IsInValidState = ValidateStateFileContents(state1Contents);
//
// if (!state1IsInValidState)
// {
//   if (state2Contents == StateValueIndexAsString(StateValue.Idling))
//   {
//     _fileSys.WriteText(_state1, StateValueIndexAsString(StateValue.Idling));
//     return;
//   }
//
//   if (state2Contents == StateValueIndexAsString(StateValue.MakingDbBackup))
//   {
//     _fileSys.Delete(_dbBackup);
//     _fileSys.WriteText(_state1, StateValueIndexAsString(StateValue.Idling));
//     _fileSys.WriteText(_state2, StateValueIndexAsString(StateValue.Idling));
//     return;
//   }
//
//   if (state2Contents == StateValueIndexAsString(StateValue.WasUpdatingOriginalDb))
//   {
//     _fileSys.Copy(src: _dbBackup, dest: _dbOriginal, overwrite: true);
//     _fileSys.WriteText(_state1, StateValueIndexAsString(StateValue.Idling));
//     _fileSys.WriteText(_state2, StateValueIndexAsString(StateValue.Idling));
//     return;
//   }
//
//   throw new NotImplementedException("Unexpected branch reached");
// }
//
// var state2IsInValidState = ValidateStateFileContents(state2Contents);
//
// if (!state2IsInValidState)
// {
//   if (state1Contents == StateValueIndexAsString(StateValue.Idling))
//   {
//     _fileSys.WriteText(_state2, StateValueIndexAsString(StateValue.Idling));
//     return;
//   }
//
//   if (state1Contents == StateValueIndexAsString(StateValue.MakingDbBackup))
//   {
//     _fileSys.Delete(_dbBackup);
//     _fileSys.WriteText(_state1, StateValueIndexAsString(StateValue.Idling));
//     _fileSys.WriteText(_state2, StateValueIndexAsString(StateValue.Idling));
//     return;
//   }
//
//   if (state1Contents == StateValueIndexAsString(StateValue.WasUpdatingOriginalDb))
//   {
//     _fileSys.Copy(src: _dbBackup, dest: _dbOriginal, overwrite: true);
//     _fileSys.WriteText(_state1, StateValueIndexAsString(StateValue.Idling));
//     _fileSys.WriteText(_state2, StateValueIndexAsString(StateValue.Idling));
//     return;
//   }
//
//   throw new NotImplementedException("Unexpected branch reached");
// }
//
// throw new NotImplementedException("Unexpected branch reached");