describe('pull authorization map', () => {
  beforeEach(() => {
    cy.login();
    cy.visit('/pullauthorizationmap');
  });

  it('should add pull authorization entry', () => {
    cy.getdatacy('add').click();
    cy.getdatacy('entries')
      .find('tr')
      .within(() => {
        cy.get('input[formcontrolname=mpc]:first').type('my mpc');
        cy.get('.input-group > .form-control').type(
          '0d512bdcd9f169ac4c22e1574ab98e3fa4d8af78'
        );
        cy.get('input[formcontrolname=allowed]').check();
      });
    cy.getdatacy('save').click();
  });

  it('should remove pull authorization entry', () => {
    cy.getdatacy('remove').click();
    cy.getdatacy('ok').click(); // Are you sure?

    cy.get('input[formcontrolname=mpc]').should('not.exist');
  });
});
