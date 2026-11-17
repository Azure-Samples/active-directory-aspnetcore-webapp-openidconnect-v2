const copilotLogin = 'copilot-swe-agent[bot]';
const repository = 'Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2';

module.exports = async function assignIssue({ github, context, core, issueNumber }) {
  if (`${context.repo.owner}/${context.repo.repo}` !== repository ||
      !Number.isSafeInteger(issueNumber) || issueNumber <= 0) {
    throw new Error('Assignment requires the upstream sample repository and a valid issue number.');
  }
  const parameters = { ...context.repo, issue_number: issueNumber };
  const { data: issue } = await github.rest.issues.get(parameters);
  if (issue.pull_request || issue.user?.login !== 'github-actions[bot]' ||
      !/<!-- sample-maintenance:[a-f0-9]{64} -->/.test(issue.body || '')) {
    throw new Error('Only workflow-created release maintenance issues may be assigned.');
  }
  if (issue.state !== 'open' || issue.assignees?.some(assignee => assignee.login === copilotLogin)) {
    core.notice('Skipping closed or already assigned release issue.');
    return;
  }
  for await (const page of github.paginate.iterator(github.rest.issues.listEventsForTimeline, {
    ...parameters, per_page: 100
  })) {
    if (page.data.some(event => event.event === 'assigned' && event.assignee?.login === copilotLogin)) {
      core.notice('Previously assigned release issue requires an explicit maintainer restart.');
      return;
    }
  }
  const { data: assigned } = await github.request('POST /repos/{owner}/{repo}/issues/{issue_number}/assignees', {
    ...parameters,
    assignees: [copilotLogin],
    agent_assignment: { target_repo: repository, base_branch: 'master', custom_agent: 'sample-implementer' },
    headers: { 'X-GitHub-Api-Version': '2022-11-28' },
    request: { retries: 0 }
  });
  if (!assigned.assignees?.some(assignee => assignee.login === copilotLogin)) {
    throw new Error('GitHub did not confirm Copilot assignment; verify token permissions, eligibility and policy.');
  }
  core.notice(`Assigned release issue #${issueNumber} with sample-implementer targeting master.`);
};