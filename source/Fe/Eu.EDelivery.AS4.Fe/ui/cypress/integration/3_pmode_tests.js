describe('pmode tests', () => {
  beforeEach(() => cy.login());

  const testDefaultRetryReliability = (x) => {
    cy
      .get('li > a[data-toggle=tab]:contains(' + x + ')')
      .click({ force: true });

    cy.get('div[title="' + x + '"]').within(() => {
      cy.getdatacy('retry.count').should('to.be.disabled');
      cy.getdatacy('retry.interval').should('to.be.disabled');

      cy.getdatacy('retry.isEnabled').check({ force: true });

      cy.getdatacy('retry.count').should('be.enabled');
      cy.getdatacy('retry.interval').should('be.enabled');
      cy.getdatacy('retry.count').should('to.have.value', '4');
      cy.getdatacy('retry.interval').should('to.have.value', '1:00:00:00');
    });
  };

  [
    { path: 'receiving', handling: 'Message handling' },
    { path: 'receiving', handling: 'Exception handling' },
    { path: 'sending', handling: 'Receipt handling' },
    { path: 'sending', handling: 'Error handling' },
    { path: 'sending', handling: 'Exception handling' }
  ].forEach((x) => {
    it('enables ' + x.handling + ' retry on enable', () => {
      cy.visit('/pmodes/' + x.path);
      cy.getdatacy('select-pmodes').select('02-sample-pmode', { force: true });

      testDefaultRetryReliability(x.handling);
    });
  });
});
