﻿using System.Collections.Generic;

namespace Inklewriter.Parsed
{
    internal class VariableReference : Expression
    {
        public string name { 
            get {
                if (path != null && path.Count == 1)
                    return path [0];
                else
                    return null;
            } 
        }
        
        public List<string> path;

        public bool isBeatCount;

        public VariableReference (List<string> path)
        {
            this.path = path;
        }

        public override void GenerateIntoContainer (Runtime.Container container)
        {
            _runtimeVarRef = new Runtime.VariableReference (name);
            container.AddContent(_runtimeVarRef);
        }

        public override void ResolveReferences (Story context)
        {
            base.ResolveReferences (context);

            // Is it a read count?
            var parsedPath = new Path (path);
            Parsed.Object targetForCount = parsedPath.ResolveFromContext (this);
            if (targetForCount) {

                Runtime.Container countedContainer = null;
                if (targetForCount is Choice) {
                    var choiceTarget = (Choice)targetForCount;
                    countedContainer = choiceTarget.runtimeContainer;
                } else if (targetForCount is FlowBase || targetForCount is Gather) {
                    countedContainer = targetForCount.runtimeObject as Runtime.Container;
                } else {
                    throw new System.Exception ("Unexpected object type");
                }

                _runtimeVarRef.pathForCount = targetForCount.runtimePath;
                _runtimeVarRef.name = null;
                if (isBeatCount) {
                    _runtimeVarRef.isBeatsSince = true;
                    countedContainer.beatIndexShouldBeCounted = true;
                } else {
                    countedContainer.visitsShouldBeCounted = true;
                }
                return;
            } 

            // Definitely a read count, but wasn't found?
            else if (path.Count > 1 || isBeatCount) {
                Error ("Could not find target for read count: " + parsedPath);
                return;
            }

            if (!context.ResolveVariableWithName (this.name, fromNode: this)) {
                Error("Unresolved variable: "+this.ToString()+" after searching: "+this.descriptionOfScope, this);
            }
        }

        public override string ToString ()
        {
            return string.Join(".", path);
        }

        Runtime.VariableReference _runtimeVarRef;
    }
}

