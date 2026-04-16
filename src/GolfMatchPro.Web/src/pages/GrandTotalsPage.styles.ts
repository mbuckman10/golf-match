import { makeStyles } from '@fluentui/react-components';

export const styles = makeStyles({
  container: {
    padding: '24px',
    maxWidth: '100%',
    margin: '0 auto',
    display: 'flex',
    flexDirection: 'column',
    gap: '24px'
  },
  filterGrid: {
    display: 'grid',
    gridTemplateColumns: 'repeat(auto-fit, minmax(150px, 1fr))',
    gap: '16px',
    marginBottom: '16px'
  },
  positiveAmount: {
    color: 'var(--golf-green-primary)',
    fontWeight: '600'
  },
  negativeAmount: {
    color: '#d6373f',
    fontWeight: '600'
  },
  total: {
    fontSize: '1.1em',
    borderTop: '2px solid #ccc',
    paddingTop: '8px'
  }
});
