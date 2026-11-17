# IdWeb Sample Maintenance Harness

This opt-in harness belongs to
Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2, targeting master.
It is a separate change from cleanup PR #831 and can operate on either layout.
It changes no sample source, dependency versions, app registrations or credentials.

## Scope

scripts/maintenance-samples.json inventories 22 original projects: 13 eligible
IdWeb applications, four support UI-test projects and five excluded B2C/sovereign
projects. Excluded files may disappear without breaking discovery. Missing
eligible/support projects and new uncataloged projects require review. The six
cataloged Microsoft.Identity.Web package IDs are dispatched individually; a
Microsoft.Identity.Client release is rejected, not mapped transitively to IdWeb.
Coordinated multi-package upgrades require explicit scope approval rather than
silently upgrading the entire IdWeb family or adding MSAL references.

## Activation

Merge/review this harness independently of the cleanup. An administrator must
explicitly configure MAINTENANCE_DISPATCH_ACTOR, MAINTENANCE_PACKAGE and
MAINTENANCE_TARGET_VERSION Actions variables on the upstream repository. The
package/version variables pin the next approved setup acquisition; dispatch must
match them. Keep one approved target in flight until its agent setup completes.
Do not change these variables underneath an active session. The agent checks its
original manifest against the assigned issue and blocks any mismatch.

After merge, manually dispatch release-maintenance.yml on master with package,
version, official release_notes_url and optional event_key. There is no schedule,
release subscription, ADO registry change or live dispatch in this PR. No actor is
enabled by default. Stable event keys are SHA256 of lowercase repository, '|',
lowercase package, '|', exact version. Open AND closed workflow-created release
issues are deduplicated. Set assign_copilot only after enabling the cloud agent
and storing COPILOT_ASSIGNMENT_TOKEN as an Actions secret. Use a user token with
GitHub's documented Copilot-assignment permissions for this repository; an
Issues-only token is not assumed sufficient. No secret value belongs in issues,
logs, source or sample configuration. Without assignment, a maintainer may inspect
the issue and explicitly assign sample-implementer later.

Workflow execution approval stays enabled according to existing repository policy.
Human merge approval is separate. This PR changes neither policy and does not
enable auto-merge. Fork PR workflows receive no assignment token. Hosted PR checks
may require maintainer approval; their existence is not proof they ran.

## Evidence And Execution

Copilot setup runs offline checks and, only for the configured exact package/version,
downloads official NuGet archives and fetches the exact IdWeb version tags into
ignored .maintenance-evidence. It does NOT run SDK code or authenticate to a tenant.
NuGet repository commit metadata must equal the corresponding tag commit for both
the target and every applicable baseline. A missing historical tag or mismatch
blocks the ENTIRE setup; this initial harness deliberately has no partial-baseline
fallback. Preview/floating versions, SDK main and source substitution are unsupported.

The immutable manifest records original project references/frameworks, package
hashes, source commits and full src/tests comparison hashes per baseline. The
planner reads bounded relevant source/contracts/tests and discovers improvements
across the entire range. The implementer applies only justified scoped edits and
invokes the independent read-only reviewer. AGENTS.md defines the required matrix,
role handoffs, one-repair/two-review budget and blocked/partial outcomes. Manifest
immutability is checked for accidental changes; it is not a cryptographic sandbox
against an agent able to edit its own workspace. Human diff review remains required.

Run offline checks with PowerShell 7:

```powershell
./scripts/Test-Maintenance.ps1
./scripts/Validate-Maintenance.ps1
```

An actual update additionally requires ORIGINAL evidence validation:

```powershell
./scripts/Validate-Maintenance.ps1 -EvidenceManifest .maintenance-evidence/manifest.json -ExpectedVersion <exact-version>
```

Only on an approved hosted build machine, add -Build to build all 13 retained
applications. maintenance-validation.yml provides those hosted builds and offline
regression checks; the existing .NET solution workflow remains unchanged. This
harness PR has not run local builds/restores, downloaded packages/source evidence,
retrieved secrets, provisioned identities, dispatched a release or performed live
authentication. Live sign-in, Graph calls, caching, authorization, claims challenges
and existing UI tests remain NOT RUN automatically and require approved test
identities and a separately reviewed basic-flow test recipe. Static/build checks
alone are not end-to-end rollout acceptance or grounds for automatic merging.