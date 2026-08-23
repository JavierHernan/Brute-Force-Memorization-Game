//have object with key:values consisting of ports as the key and protocols abbreviation + name as the value
//the last key/value pair is add to a new array?
//random item in that array is presented. The protocol abbreviation is presented.
//Enter the port for that protocol correctly
    //if all items in that array(maybe object) have been answered correctly, 
    //add a new key/value from original to second array, and present that new item as question.
    //if correct, repeat.
        //if incorrect, present the right answer for 3 seconds. 
        //Then, start from the beginning of the 2nd array (or do random presentation), 
        //keep that new item in the array.
        //once presented, if answered incorrectly, repeat process until it is answere correctly.
        //If answered correcty, add new item, etc until original object is empty.

        //https://github.com/JavierHernan/Brute-Force-Memorization-Game/issues?q=is%3Aissue+updated%3A%3E%40today-1w+sort%3Aupdated-desc

        // #!/usr/bin/env node
/**
 * Ports & Protocols Flashcard CLI Game
 * Brute-force memorization through repetition + error-driven review.
 *
 * Usage: node ports-protocols-game.js
 * Type "quit" at any prompt to exit.
 */

const readline = require('readline');

// ─── Protocol data ───────────────────────────────────────────────────────────
const PROTOCOLS = [
  { abbr: 'HTTP', full: 'HyperText Transfer Protocol', ports: [80] },
  { abbr: 'HTTPS', full: 'HyperText Transfer Protocol Secure', ports: [443] },
  { abbr: 'SMTP', full: 'Simple Mail Transfer Protocol', ports: [25] },
  { abbr: 'SMTPS', full: 'Simple Mail Transfer Protocol Secure', ports: [465, 587] },
  { abbr: 'POP3', full: 'Post Office Protocol version 3', ports: [110] },
  { abbr: 'POP3S', full: 'Post Office Protocol version 3 Secure', ports: [995] },
  { abbr: 'IMAP', full: 'Internet Message Access Protocol', ports: [143] },
  { abbr: 'IMAPS', full: 'Internet Message Access Protocol Secure', ports: [993] },
  { abbr: 'SSH', full: 'Secure Shell', ports: [22] },
  { abbr: 'SFTP', full: 'SSH File Transfer Protocol', ports: [22] },
  { abbr: 'Telnet', full: 'Telecommunication Network', ports: [23] },
  { abbr: 'RDP', full: 'Remote Desktop Protocol', ports: [3389] },
  { abbr: 'FTP', full: 'File Transfer Protocol', ports: [20, 21] },
  { abbr: 'TFTP', full: 'Trivial File Transfer Protocol', ports: [69] },
  { abbr: 'DNS', full: 'Domain Name System', ports: [53] },
  { abbr: 'DHCP', full: 'Dynamic Host Configuration Protocol', ports: [67, 68] },
  { abbr: 'LDAP', full: 'Lightweight Directory Access Protocol', ports: [389] },
  { abbr: 'LDAPS', full: 'Lightweight Directory Access Protocol Secure', ports: [636] },
  { abbr: 'SQL Service (MSSQL)', full: 'Microsoft SQL Server', ports: [1433] },
  { abbr: 'SQL Service (MySQL)', full: 'MySQL Server', ports: [3306] },
  { abbr: 'SNMP', full: 'Simple Network Management Protocol', ports: [161, 162] },
  { abbr: 'SysLog', full: 'System Logging Protocol', ports: [514] },
  { abbr: 'NTP', full: 'Network Time Protocol', ports: [123] },
  { abbr: 'SIP', full: 'Session Initiation Protocol', ports: [5060, 5061] },
  { abbr: 'H.323', full: null, ports: [1720] },
  { abbr: 'SMB and CIFS', full: 'Server Message Block / Common Internet File System', ports: [445] },
  { abbr: 'NetBIOS and NetBT', full: 'Network Basic Input/Output System', ports: [137, 138, 139] },
];

// ─── Helpers ─────────────────────────────────────────────────────────────────
function shuffle(array) {
  const a = [...array];
  for (let i = a.length - 1; i > 0; i--) {
    const j = Math.floor(Math.random() * (i + 1));
    [a[i], a[j]] = [a[j], a[i]];
  }
  return a;
}

function normalizePorts(input) {
  // Accept numbers separated by /, -, ,, or whitespace
  const nums = String(input)
    .split(/[\/\-\s,]+/)
    .map((s) => parseInt(s.trim(), 10))
    .filter((n) => !isNaN(n) && n > 0 && n <= 65535);
  return [...new Set(nums)].sort((a, b) => a - b);
}

function portsEqual(a, b) {
  if (a.length !== b.length) return false;
  for (let i = 0; i < a.length; i++) {
    if (a[i] !== b[i]) return false;
  }
  return true;
}

function canonicalPorts(ports) {
  return [...ports].sort((a, b) => a - b).join('/');
}

function displayName(proto) {
  return proto.full ? `${proto.abbr} - ${proto.full}` : proto.abbr;
}

// ─── Readline setup ──────────────────────────────────────────────────────────
const rl = readline.createInterface({
  input: process.stdin,
  output: process.stdout,
});

function question(prompt) {
  return new Promise((resolve) => rl.question(prompt, resolve));
}

// ─── Core ask function ───────────────────────────────────────────────────────
/**
 * Asks for the ports of a protocol.
 * Returns: true (correct), false (incorrect, but acknowledged), or 'quit'
 */
async function askProtocol(proto, isReview = false) {
  const name = displayName(proto);
  const expected = [...proto.ports].sort((a, b) => a - b);
  const canon = canonicalPorts(expected);

  console.log('');
  if (isReview) {
    console.log(`REVIEW → Protocol: ${name}`);
  } else {
    console.log(`Protocol: ${name}`);
  }
  console.log('Enter the port number(s) (separate multiple with /, any order is fine):');

  const answer = (await question('> ')).trim();
  if (answer.toLowerCase() === 'quit') return 'quit';

  const userPorts = normalizePorts(answer);

  if (portsEqual(userPorts, expected)) {
    if (isReview) {
      console.log('  ✓ Correct');
    }
    return true;
  }

  // Incorrect
  console.log(`\n✗ Incorrect. The correct answer is: ${canon}`);

  // Force the user to type the correct answer (acknowledgment)
  while (true) {
    const ack = (await question('Type the correct port(s) to acknowledge: ')).trim();
    if (ack.toLowerCase() === 'quit') return 'quit';
    if (portsEqual(normalizePorts(ack), expected)) {
      break;
    }
    console.log('  That does not match. Please type the correct port(s).');
  }

  await question('Press Enter to continue...');
  return false;
}

// ─── Review helper ───────────────────────────────────────────────────────────
async function doReview(mastered) {
  if (mastered.length === 0) return 'ok';

  console.log('\n────────────────────────────────────────');
  console.log('  Reviewing previously mastered protocols');
  console.log('  (shuffled order)');
  console.log('────────────────────────────────────────');

  const reviewList = shuffle(mastered);
  for (const proto of reviewList) {
    const result = await askProtocol(proto, true);
    if (result === 'quit') return 'quit';
    // Whether correct or forced-ack, continue to next
  }

  console.log('\n── Review complete ──');
  return 'ok';
}

// ─── Main game loop ──────────────────────────────────────────────────────────
async function main() {
  console.log('╔══════════════════════════════════════════════════════╗');
  console.log('║     Ports & Protocols Memorization Game              ║');
  console.log('║     Brute-force learning through repetition          ║');
  console.log('╚══════════════════════════════════════════════════════╝');
  console.log('');
  console.log('• Protocols are presented one at a time in random order.');
  console.log('• Type the port(s) using / as separator (e.g. 20/21).');
  console.log('• On a miss you must type the correct answer, then re-answer');
  console.log('  every previously mastered protocol before retrying.');
  console.log('• Score (streak) resets on any incorrect answer.');
  console.log('• Type "quit" at any prompt to exit.\n');

  let remaining = [...PROTOCOLS];
  let mastered = [];
  let streak = 0;
  let pendingRetry = null;
  const total = PROTOCOLS.length;

  while (mastered.length < total) {
    // Decide which protocol to ask next
    let proto;
    if (pendingRetry) {
      proto = pendingRetry;
    } else if (remaining.length > 0) {
      // Pick a random remaining protocol (do not remove yet)
      const idx = Math.floor(Math.random() * remaining.length);
      proto = remaining[idx];
    } else {
      // Should not happen, but safety
      break;
    }

    const isRetry = pendingRetry !== null;
    if (isRetry) {
      console.log('\n>>> Retrying the protocol you previously missed <<<');
    }

    const result = await askProtocol(proto, false);

    if (result === 'quit') {
      console.log('\nExiting game. Goodbye!');
      break;
    }

    if (result === true) {
      // Success
      if (pendingRetry) {
        pendingRetry = null;
      } else {
        // Remove from remaining
        remaining = remaining.filter((p) => p !== proto);
      }

      // Avoid duplicate if somehow already present
      if (!mastered.includes(proto)) {
        mastered.push(proto);
      }

      streak += 1;
      console.log(`\n✓ Correct!  Streak: ${streak}  |  Mastered: ${mastered.length}/${total}`);
    } else {
      // Miss
      streak = 0;
      console.log(`\nStreak reset to 0.  Mastered so far: ${mastered.length}/${total}`);

      // Force review of everything already mastered
      const reviewResult = await doReview(mastered);
      if (reviewResult === 'quit') {
        console.log('\nExiting game. Goodbye!');
        break;
      }

      // Set up retry of the failed protocol on next iteration
      pendingRetry = proto;
      console.log('\nNow retrying the protocol you missed...\n');
    }
  }

  if (mastered.length === total) {
    console.log('\n╔══════════════════════════════════════════════════════╗');
    console.log('║  🎉  Congratulations! You mastered every protocol!   ║');
    console.log('╚══════════════════════════════════════════════════════╝');
    console.log(`Final streak: ${streak}`);
    console.log(`Total protocols: ${total}`);
  }

  rl.close();
}

main().catch((err) => {
  console.error('Unexpected error:', err);
  rl.close();
  process.exit(1);
});

//node main.js

//quit