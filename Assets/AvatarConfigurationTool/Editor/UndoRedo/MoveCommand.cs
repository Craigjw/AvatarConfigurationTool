using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ACT.UndoRedo
{
    public class MoveCommand : ICommand<Bone>
    {        
        public MoveCmd Value { get; set; }
        

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="bone">Bone to Move</param>
        public MoveCommand(Bone bone)
        {
            Value = AddCmd(bone);            
        }
        /// <summary>
        /// Create a new Move command
        /// </summary>
        /// <param name="bone"></param>
        /// <returns></returns>
        public MoveCmd AddCmd(Bone bone)
        {
            MoveCmd cmd = new MoveCmd(bone);
            
            return cmd;
        }
        /// <summary>
        /// Record a Do command
        /// </summary>
        /// <param name="input">Bone to record</param>
        public void Do(Bone input)
        {
            var record = new MoveCmd(input);
            Value = record;

        }
        /// <summary>
        /// Record an Undo command
        /// </summary>
        /// <param name="input">Bone to Undo</param>
        public void Undo(Bone input)
        {
            var record = new MoveCmd(input);
        }
    }
}
