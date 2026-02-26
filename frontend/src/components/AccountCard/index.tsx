import { Eye, EyeOff, ArrowUpRight, ArrowDownLeft } from 'lucide-react';
import { useState } from 'react';
import { formatDistanceToNow } from 'date-fns';

interface Transaction {
  id: number; type: 'debit' | 'credit' | 'transfer'; amount: number;
  description: string; category?: string; createdAt: string; balanceAfter: number;
}

interface Account {
  id: number; accountNumber: string; type: 'checking' | 'savings' | 'investment';
  balance: number; currency: string; transactions: Transaction[];
}

const TYPE_GRADIENTS = {
  checking: 'from-indigo-600 to-purple-700',
  savings: 'from-emerald-600 to-teal-700',
  investment: 'from-orange-600 to-rose-700',
};

const TYPE_LABELS = { checking: 'Checking Account', savings: 'Savings Account', investment: 'Investment Account' };

function mask(num: string) {
  return `**** **** **** ${num.slice(-4)}`;
}

export default function AccountCard({ account }: { account: Account }) {
  const [showBalance, setShowBalance] = useState(true);

  const formatter = new Intl.NumberFormat('en-US', {
    style: 'currency', currency: account.currency
  });

  return (
    <div className="space-y-4">
      {/* Card */}
      <div className={`bg-gradient-to-br ${TYPE_GRADIENTS[account.type]} rounded-2xl p-6 text-white shadow-xl relative overflow-hidden`}>
        <div className="absolute top-0 right-0 w-48 h-48 bg-white/5 rounded-full -translate-y-1/2 translate-x-1/2" />
        <div className="absolute bottom-0 left-0 w-32 h-32 bg-white/5 rounded-full translate-y-1/2 -translate-x-1/2" />
        <div className="relative z-10">
          <div className="flex justify-between items-start mb-8">
            <div>
              <p className="text-white/60 text-xs uppercase tracking-widest">{TYPE_LABELS[account.type]}</p>
              <p className="text-sm mt-1 font-mono">{mask(account.accountNumber)}</p>
            </div>
            <button onClick={() => setShowBalance(!showBalance)} className="text-white/60 hover:text-white">
              {showBalance ? <Eye size={18} /> : <EyeOff size={18} />}
            </button>
          </div>
          <div>
            <p className="text-white/60 text-xs">Available Balance</p>
            <p className="text-3xl font-bold mt-1">
              {showBalance ? formatter.format(account.balance) : '••••••'}
            </p>
          </div>
        </div>
      </div>

      {/* Recent Transactions */}
      <div className="bg-white rounded-xl shadow-sm border border-gray-100 overflow-hidden">
        <div className="px-5 py-4 border-b border-gray-100 flex justify-between items-center">
          <h3 className="font-semibold text-gray-800 text-sm">Recent Transactions</h3>
          <button className="text-xs text-indigo-600 hover:text-indigo-700 font-medium">View all</button>
        </div>
        <div className="divide-y divide-gray-50">
          {account.transactions.slice(0, 5).map(tx => (
            <div key={tx.id} className="flex items-center gap-3 px-5 py-3">
              <div className={`w-9 h-9 rounded-full flex items-center justify-center ${tx.type === 'credit' ? 'bg-green-50' : 'bg-red-50'}`}>
                {tx.type === 'credit'
                  ? <ArrowDownLeft size={16} className="text-green-500" />
                  : <ArrowUpRight size={16} className="text-red-500" />}
              </div>
              <div className="flex-1 min-w-0">
                <p className="text-sm font-medium text-gray-800 truncate">{tx.description}</p>
                <p className="text-xs text-gray-400">{formatDistanceToNow(new Date(tx.createdAt), { addSuffix: true })}</p>
              </div>
              <span className={`text-sm font-semibold ${tx.type === 'credit' ? 'text-green-600' : 'text-red-500'}`}>
                {tx.type === 'credit' ? '+' : '-'}{formatter.format(tx.amount)}
              </span>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
