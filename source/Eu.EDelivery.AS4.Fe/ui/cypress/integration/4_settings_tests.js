describe('save retryReliability.pollingInterval', () => {
  beforeEach(() => cy.login());

  it('persist the pollingInterval', () => {
    cy.visit('/settings/runtime');

    cy.getdatacy('pollingInterval').type('{selectall}00:00:10');
    cy.getdatacy('save').click();
    cy.reload();

    cy.getdatacy('pollingInterval').should('have.value', '00:00:10');
  });
});
