# ADR 0001: Shader Graph First

## Status

Accepted

## Context

This repository is intended to become a focused MCP integration for Unity Shader Graph workflows. A broad Unity MCP would dilute the first milestone and push the most important graph-editing behavior behind unrelated tooling.

## Decision

We will keep the repository Shader Graph focused and model milestone 1 around graph creation, inspection, property edits, node insertion, port connection, and save operations.

## Consequences

- The contract stays narrow enough to implement and test quickly.
- Tool names and payloads can stay stable around one domain.
- Future expansion to other Unity features can happen later without blurring the first product boundary.
