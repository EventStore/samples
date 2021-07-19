# Contributing to EventStoreDB samples

## Before you send Pull Request

1. Contact the maintainers via the [Discuss forum](https://discuss.eventstore.com/), [GitHub Issue](https://github.com/EventStore/samples/issues/new) or the [GitHub Discussions](https://github.com/EventStore/samples/discussions) to make sure that this is issue or bug should be handled with proposed way. Send details of your case and explain the details of the proposed solution.
2. Once you get approval from one of the maintainers, you can start to work on your code change.
3. After your changes are ready, make sure that you covered your case with automated tests and verify that you have limited the number of breaking changes to a bare minimum.
4. Make sure that your code is compiling and all automated tests are passing.

## After you have sent Pull Request

1. Make sure that you applied or answered all the feedback from the maintainers.
2. We're trying to be as much responsive as we can, but if we didn't respond to you, feel free to ping us by tagging us in the Issue or Pull Request.
3. Pull request will be merged when you get approvals from at least 2 of the maintainers (and no rejection from others). 

## Working with the Git

We're using `main` as the main development branch. It contains all the recent. Specific releases are tagged from the main branches commits. 

We attempt to do our best to ensure that the history remains clean and to do so, we generally ask contributors to squash their commits into a set or single logical commit.

To contribute to EventStoreDB samples:

1. Fork the repository.
2. Create a feature branch from the `main` (or release) branch.
3. It's recommended using rebase strategy for feature branches (see more in [Git documentation](https://git-scm.com/book/en/v2/Git-Branching-Rebasing)). Having that, we highly recommend using clear commit messages. Commits should also represent the unit of change.
4. Before sending PR to ensure that you rebased the latest source branch from the main repository.
5. When you're ready to create the [Pull Request on GitHub](https://github.com/EventStore/samples/compare).

## Code style

Coding rules are set up in the project files to be automatically applied (e.g. with `.editorconfig` or `.prettierrc.json`). Unless you disabled it manually, it should be automatically applied by IDE after opening the project. We also recommend turning automatic formatting on saving to have all the rules applied.

## Licensing and legal rights

By contributing to EventStoreDB:

1. You assert that contribution is your original work.
2. You assert that you have the right to assign the copyright for the work.
3. You are accepting the [License](LICENSE.md).
