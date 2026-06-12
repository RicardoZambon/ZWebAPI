const isValidation = process.env.SEMANTIC_RELEASE_VALIDATION === 'true';

const plugins = [
  [
    '@semantic-release/commit-analyzer',
    {
      preset: 'angular',
      releaseRules: [
        { type: 'bug', release: 'patch' },
        { type: 'fix', release: 'patch' },
        { type: 'refactor', release: 'patch' },
        { type: 'feat', release: 'minor' },
        { type: 'breaking-changes', release: 'major' },
        { scope: 'no-release', release: false },
      ],
    },
  ],
  '@semantic-release/release-notes-generator',
];

if (!isValidation) {
  plugins.push(
    '@semantic-release/github',
    [
      '@semantic-release/git',
      {
        assets: ['ZWebAPI/ZWebAPI.csproj'],
        message: 'chore(release): ${nextRelease.version} [skip ci]\n\n${nextRelease.notes}',
      },
    ],
  );
}

module.exports = {
  branches: isValidation
    ? [process.env.GITHUB_HEAD_REF || process.env.GITHUB_REF_NAME || 'main']
    : [
        'v+([0-9])?(.{+([0-9]),x}).x',
        'main',
        { name: 'beta', prerelease: true },
        { name: 'alpha', prerelease: true },
      ],
  plugins,
};
