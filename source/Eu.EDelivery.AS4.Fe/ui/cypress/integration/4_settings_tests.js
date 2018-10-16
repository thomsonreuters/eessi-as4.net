describe('save retryReliability.pollingInterval', () => {
  beforeEach(() => cy.login());

  it('persist the pollingInterval', () => {
    cy.visit('/settings/runtime');

    cy.getdatacy('pollingInterval').type('{selectall}00:00:10');
    cy.getdatacy('base-save').click();
    cy.reload();

    cy.getdatacy('pollingInterval').should('have.value', '00:00:10');
  });

  it('persist the pull send authorization map path', () => {
    cy.visit('/settings/runtime');
    const fixture = './my-security-path/pull_authorization_map.xml';
    cy.getdatacy('authorizationMap').type('{selectall}' + fixture, {
      force: true
    });
    cy.getdatacy('pullsend-save').click({ force: true });
    cy.reload();

    cy.getdatacy('authorizationMap').should('have.value', fixture);
  });

  it('persist the submit payload retrieval path', () => {
    cy.visit('/settings/runtime');
    const fixture = './my-attachment-path/';
    cy.getdatacy('payloadRetrieval').type('{selectall}' + fixture, {
      force: true
    });
    cy.getdatacy('submit-save').click({ force: true });
    cy.reload();

    cy.getdatacy('payloadRetrieval').should('have.value', fixture);
  });
});
