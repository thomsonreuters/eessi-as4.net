describe('smp configuration should support CRUD actions', () => {
  beforeEach(() => {
    cy.login();
    cy.visit('/smpconfiguration');
  });

  const shouldHaveRowsLength = (l) => {
    cy
      .getdatacy('smp-configs')
      .find('tr')
      .should('have.length', l);
  };

  it.skip('should create a new smp configuration', () => {
    shouldHaveRowsLength(0);

    cy.getdatacy('smp-add').click();
    cy.getdatacy('smp-toparty').type('my to party');
    cy.getdatacy('smp-partytype').type('my party type');
    cy.getdatacy('smp-partyrole').type('my party role');
    cy.getdatacy('smp-url').type('my url');
    cy.getdatacy('ok').click();

    shouldHaveRowsLength(1);
  });

  it.skip('should change an existing smp configuration', () => {
    cy.getdatacy('smp-edit').click({ force: true });
    cy.getdatacy('smp-partyrole').type('{selectall}other role');
    cy.getdatacy('ok').click();

    shouldHaveRowsLength(1);
    cy.getdatacy('smp-row-partyrole').should('have.text', 'other role');
  });

  it.skip('should delete an existing smp configuration', () => {
    shouldHaveRowsLength(1);

    cy.getdatacy('smp-delete').click();
    cy.getdatacy('ok').click();

    shouldHaveRowsLength(0);
  });
});
