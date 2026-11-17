const test = require('node:test');
const assert = require('node:assert/strict');
const assignIssue = require('./assign-maintenance-issue.cjs');
const bot = 'copilot-swe-agent[bot]';

function fixture(overrides = {}, events = []) {
  const calls = [];
  const issue = { state: 'open', user: { login: 'github-actions[bot]' },
    body: `<!-- sample-maintenance:${'a'.repeat(64)} -->`, assignees: [], ...overrides };
  const github = {
    rest: { issues: { get: async () => ({ data: issue }), listEventsForTimeline() {} } },
    paginate: { async *iterator() { yield { data: [] }; yield { data: events }; } },
    request: async (route, payload) => {
      calls.push({ route, payload });
      return { data: { assignees: [{ login: bot }] } };
    }
  };
  return { calls, args: { github, context: { repo: { owner: 'Azure-Samples',
    repo: 'active-directory-aspnetcore-webapp-openidconnect-v2' } },
    core: { notice() {} }, issueNumber: 42 } };
}

test('assigns the upstream master branch and bounded implementer without retries', async () => {
  const { calls, args } = fixture();
  await assignIssue(args);
  assert.equal(calls.length, 1);
  assert.deepEqual(calls[0].payload.agent_assignment, {
    target_repo: 'Azure-Samples/active-directory-aspnetcore-webapp-openidconnect-v2',
    base_branch: 'master', custom_agent: 'sample-implementer'
  });
  assert.equal(calls[0].payload.request.retries, 0);
});

for (const [name, overrides, events] of [
  ['closed', { state: 'closed' }, []],
  ['already assigned', { assignees: [{ login: bot }] }, []],
  ['previously assigned on a later timeline page', {}, [{ event: 'assigned', assignee: { login: bot } }]]
]) {
  test(`does not restart ${name}`, async () => {
    const { calls, args } = fixture(overrides, events);
    await assignIssue(args);
    assert.equal(calls.length, 0);
  });
}
for (const [name, overrides] of [
  ['human-created issue', { user: { login: 'someone' } }],
  ['unmarked issue', { body: 'ordinary issue' }],
  ['pull request', { pull_request: {} }]
]) {
  test(`rejects ${name}`, async () => {
    const { args } = fixture(overrides);
    await assert.rejects(assignIssue(args), /Only workflow-created/);
  });
}
test('rejects fork or invalid issue number', async () => {
  const { args } = fixture();
  await assert.rejects(assignIssue({ ...args, issueNumber: 0 }), /upstream sample/);
  args.context.repo.owner = 'hossnrabbit';
  await assert.rejects(assignIssue(args), /upstream sample/);
});
test('fails if GitHub does not confirm assignment', async () => {
  const { args } = fixture();
  args.github.request = async () => ({ data: { assignees: [] } });
  await assert.rejects(assignIssue(args), /did not confirm/);
});