// The MIT License (MIT) - https://gist.github.com/bcatcho/1926794b7fd6491159e7
// Copyright (c) 2015 Brandon Catcho
using System;

// Place this file in any folder that is or is a descendant of a folder named "Scripts"
namespace CatchCo  
{
   // Restrict to methods only
   [AttributeUsage(AttributeTargets.Method)]
   public class ExposeMethodInEditorAttribute : Attribute
   {
   }
}