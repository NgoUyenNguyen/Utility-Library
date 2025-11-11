# Behaviour Frameworks

---

Behaviour Frameworks provide structured approaches to manage and organize game object behaviors and state transitions in
Unity applications. These frameworks help create more maintainable and scalable behavior systems for game entities.

## Overview

This library includes two main types of behavior frameworks:

1. [Classic State Machine](StateMachine/StateMachine.md) - A traditional finite state machine implementation for
   managing simple state transitions
2. [Stack-based State Machine](StackStateMachine/StackStateMachine.md) - A state machine implementation for managing
   its states using a stack data structure.
3. [Hierarchy State Machine](HierarchyStateMachine/HierarchyStateMachine.md) - An advanced state machine supporting
   nested states and complex state hierarchies

## Benefits and Use Cases

- Organized behavior management for game entities
- Clear separation of different states and transitions
- Reduced complexity in behavior implementation
- Easier maintenance and debugging
- Suitable for AI behaviors, character controllers, game flow management, and UI state handling

Choose the appropriate framework based on your needs:

- Use Classic State Machine for easy approach with simpler behaviors with clear, distinct states
- Use Stack-based State Machine for behaviors requiring a stack data structure for state management
- Use Hierarchy State Machine for complex behaviors requiring nested states and shared behavior inheritance
