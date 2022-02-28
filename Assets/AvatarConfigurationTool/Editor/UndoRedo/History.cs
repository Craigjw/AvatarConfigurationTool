using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ACT.UndoRedo
{
    [Serializable]
    public class History
    {
        public Stack<MoveCmd> undo;
        public Stack<MoveCmd> redo;

        public int UndoCount { get { return undo.Count; } }
        public int RedoCount { get { return redo.Count; } }
        /// <summary>
        /// Constructor
        /// </summary>
        public History()
        {
            Reset();
        }
        /// <summary>
        /// Reset the undo/redo history stacks
        /// </summary>
        public void Reset()
        {
            undo = new Stack<MoveCmd>();
            redo = new Stack<MoveCmd>();
        }
        /// <summary>
        /// Record and Do move command
        /// </summary>
        /// <param name="cmd"></param>
        public void Do(MoveCmd cmd)
        {
            undo.Push(cmd);
            redo.Clear();
        }
        /// <summary>
        /// Pops the most resent Undo command from the stack and pushes this onto the redo stack
        /// </summary>
        public void Undo()
        {
            if (UndoCount > 0)
            {
                //Debug.Log("Undo: " + undo.Count);
                var cmd = undo.Pop();
                cmd.Undo();
                redo.Push(cmd);
            }
        }
        /// <summary>
        /// Pops the most recent redo command from the stack and pushes this onto the undo stack
        /// </summary>
        public void Redo()
        {
            if (redo.Count > 0)
            {
                var cmd = redo.Pop();
                cmd.Redo();
                undo.Push(cmd);
            }
        }        
    }
}