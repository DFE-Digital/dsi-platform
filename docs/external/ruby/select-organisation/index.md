# What is select organisation?

`DfeSignIn::SelectOrganisation` is a Ruby library designed to simplify interaction with the DfE public API, specifically the "select organisation" endpoints. The overarching goal is to allow a user to "select an organisation" they wish to use.

`DfeSignIn::SelectOrganisation::Middleware` is a Rack middleware, designed to manage the "selection organisation" flow. It intercepts specific URL paths to perform two primary functions:

1. Initiating a session with the DfE Public API to enable users to select an organisation.
2. Processing the response once the user has made their selection.

This library provides a flexible, pluggable interface that allows developers to customise its behavior to meet specific service requirements. It includes example implementations that demonstrate how to achieve these tasks.

> **Note:** As with any code examples, you should always follow your established guidelines for software development, along with system and data security.
